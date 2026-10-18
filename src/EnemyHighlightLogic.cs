using System;
using System.Collections.Generic;
using System.Reflection;
using SandBox.Missions.MissionLogics.Hideout;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace EnemyHighlighter
{
    /// <summary>
    /// Added to every single-player scene. While the highlight key is held (or after it was pressed, in toggle
    /// mode) every enemy within the set distance gets an outline, using the engine's own contour effect.
    /// It only draws outlines: no AI, combat or save data is touched.
    /// </summary>
    public class EnemyHighlightLogic : MissionLogic
    {
        // How often the list of outlined enemies is rebuilt while the highlight is on.
        private const float RefreshInterval = 0.1f;
        // Every so often the outline is set again on everyone, in case something else in the game cleared it.
        private const float ReassertInterval = 1f;

        // Toggle state carried from one scene to the next ("Keep toggle between scenes").
        private static bool _sharedToggle;
        // The key a conflict warning was last shown for, so it is shown once per key and game session.
        private static InputKey _warnedKey = InputKey.Invalid;

        private readonly SceneKind _kind;
        private readonly bool _isStealthScene;
        private readonly Dictionary<Agent, uint> _outlined = new Dictionary<Agent, uint>();
        private readonly List<Agent> _wanted = new List<Agent>();
        private readonly HashSet<Agent> _wantedSet = new HashSet<Agent>();
        private readonly List<Agent> _toClear = new List<Agent>();
        private readonly List<KeyValuePair<float, Agent>> _candidates = new List<KeyValuePair<float, Agent>>();

        private bool _toggledOn, _keyWasDown, _wasActive, _failed, _announced;
        private bool _needRelease;        // ignore the key until it has been seen up (after the window regains focus)
        private bool _focusLostWhileHeld; // the current press must not count as a toggle (Alt+Tab)
        private float _refreshTimer, _reassertTimer;
        private bool _lastThroughWalls, _agentErrorLogged;
        private Agent _focusedAgent;      // the agent the player is looking at, which the game itself may outline
        private HideoutCinematicController _hideoutCinematic;
        private HideoutAmbushBossFightCinematicController _ambushCinematic;

        public EnemyHighlightLogic(SceneKind kind, bool isStealthScene)
        {
            _kind = kind;
            _isStealthScene = isStealthScene;
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _toggledOn = EhSettings.KeepToggleBetweenScenes && _sharedToggle;
            try { ScreenManager.FocusGained += OnWindowFocusGained; }
            catch (Exception) { /* focus tracking is a nicety */ }
        }

        public override void AfterStart()
        {
            base.AfterStart();
            _hideoutCinematic = Mission.GetMissionBehavior<HideoutCinematicController>();
            _ambushCinematic = Mission.GetMissionBehavior<HideoutAmbushBossFightCinematicController>();
        }

        public override void OnMissionTick(float dt)
        {
            if (_failed) return;
            try
            {
                if (!_announced)
                {
                    _announced = true;
                    Log.Debug($"scene: {_kind}{(_isStealthScene ? " (stealth)" : "")}, mode {Mission.Mode}, highlight {(EhSettings.IsAllowedIn(_kind) ? "allowed" : "off for this kind of scene")}, key {EhSettings.HighlightKey}, {(EhSettings.ToggleMode ? "toggle" : "hold")}");
                    WarnIfKeyConflicts();
                }

                if (!EhSettings.Enabled)
                {
                    _keyWasDown = false;
                    Deactivate();
                    return;
                }

                // The key is read even in a cutscene or stealth part, so a toggle made there still counts afterwards.
                bool active = ReadKey();
                if (!active || !EhSettings.IsAllowedIn(_kind) || IsBlockedNow())
                {
                    Deactivate();
                    return;
                }

                _refreshTimer -= dt;
                _reassertTimer -= dt;
                if (!_wasActive || _refreshTimer <= 0f)
                {
                    bool reassert = !_wasActive || _reassertTimer <= 0f;
                    Refresh(reassert);
                    _refreshTimer = RefreshInterval;
                    if (reassert) _reassertTimer = ReassertInterval;
                    if (!_wasActive) Log.Debug($"highlight on: {_outlined.Count} outlined");
                }
                _wasActive = true;
            }
            catch (Exception e)
            {
                _failed = true;
                SafeClearAll();
                Log.Error("OnMissionTick", e);
            }
        }

        // ------------------------------------------------------------------ key handling

        /// <summary>True while the highlight should be showing.</summary>
        private bool ReadKey()
        {
            InputKey key = EhSettings.HighlightKey;
            bool focused = WindowFocus.IsFocused;
            bool down = focused && key != InputKey.Invalid && Input.IsKeyDown(key);

            // After coming back to the game (often with Alt still held from Alt+Tab, or with Windows having
            // missed the key going up) the key only counts again once it has been seen released.
            if (_needRelease)
            {
                if (!down) _needRelease = false;
                down = false;
            }
            if (!focused && _keyWasDown) _focusLostWhileHeld = true;

            if (!EhSettings.ToggleMode)
            {
                _keyWasDown = down;
                return down;
            }

            // Toggle when the key is released, not when it goes down: Alt+Tab and Alt+F4 also start with Alt
            // going down, and the window losing focus before the release cancels the toggle.
            if (!down && _keyWasDown)
            {
                if (!_focusLostWhileHeld)
                {
                    _toggledOn = !_toggledOn;
                    _sharedToggle = _toggledOn;
                    if (EhSettings.ShowToggleMessage)
                    {
                        Log.Info(_toggledOn
                                ? new TextObject("{=ehl_msg_on}Enemy highlight on")
                                : new TextObject("{=ehl_msg_off}Enemy highlight off"),
                            _toggledOn ? Colors.Green : Colors.Gray);
                    }
                }
                _focusLostWhileHeld = false;
            }
            if (!down && !_keyWasDown) _focusLostWhileHeld = false;
            _keyWasDown = down;
            return _toggledOn;
        }

        private void OnWindowFocusGained()
        {
            _needRelease = true;
            if (_keyWasDown) _focusLostWhileHeld = true;
        }

        /// <summary>Once per key and game session: say which game controls also use the highlight key.</summary>
        private static void WarnIfKeyConflicts()
        {
            InputKey key = EhSettings.HighlightKey;
            if (key == _warnedKey || !EhSettings.WarnKeyConflicts) return;
            _warnedKey = key;
            try
            {
                var actions = new List<string>();
                foreach (GameKeyContext context in HotKeyManager.GetAllCategories())
                {
                    if (context?.RegisteredGameKeys == null) continue;
                    foreach (GameKey gameKey in context.RegisteredGameKeys)
                    {
                        if (gameKey?.KeyboardKey == null || gameKey.KeyboardKey.InputKey != key) continue;
                        // Show Indicators on Left Alt is expected and harmless.
                        if (gameKey.StringId == "ShowIndicators") continue;
                        if (!actions.Contains(gameKey.StringId)) actions.Add(gameKey.StringId);
                    }
                }
                if (actions.Count == 0) return;
                Log.Info(new TextObject("{=ehl_msg_key_conflict}Enemy Highlighter: the highlight key {KEY} is also bound to {ACTIONS}. Pick another key in Mod Options if this gets in the way.")
                        .SetTextVariable("KEY", key.ToString())
                        .SetTextVariable("ACTIONS", string.Join(", ", actions)),
                    Colors.Yellow);
            }
            catch (Exception e)
            {
                Log.Debug("key conflict check failed: " + e.Message);
            }
        }

        // ------------------------------------------------------------------ when not to show

        /// <summary>Stealth parts of missions and cutscenes, unless the options allow them.</summary>
        private bool IsBlockedNow()
        {
            MissionMode mode = Mission.Mode;
            if (!EhSettings.AllowInStealth && (_isStealthScene || mode == MissionMode.Stealth))
                return true;
            if (EhSettings.HideInCutscenes)
            {
                if (mode == MissionMode.CutScene || Mission.IsInPhotoMode) return true;
                if (_hideoutCinematic != null && _hideoutCinematic.IsCinematicActive) return true;
                if (_ambushCinematic != null && _ambushCinematic.IsCinematicActive) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ outlines

        private void Refresh(bool reassert)
        {
            Agent main = Agent.Main;
            bool mainAlive = main != null && main.IsActive();
            Team playerTeam = mainAlive && IsRealTeam(main.Team) ? main.Team : Mission.PlayerTeam;
            if (!IsRealTeam(playerTeam))
            {
                ClearAll();
                return;
            }

            // Measure from the player, or from the camera once the player is down and spectating.
            Vec3 origin = mainAlive ? main.Position : Mission.GetCameraFrame().origin;
            float maxDistance = EhSettings.MaxDistance;
            float maxSq = maxDistance * maxDistance;
            int maxOutlined = EhSettings.MaxOutlined;
            bool outlineFleeing = EhSettings.OutlineFleeing;
            bool outlineHorses = EhSettings.OutlineHorses;
            bool throughWalls = EhSettings.ThroughWalls;
            uint color = EhSettings.OutlineColor;

            if (throughWalls != _lastThroughWalls)
            {
                _lastThroughWalls = throughWalls;
                reassert = true;
            }

            _candidates.Clear();
            foreach (Agent agent in Mission.Agents)
            {
                // One odd agent (spectator, cinematic extra, half-spawned) must not stop the highlight for everyone.
                try
                {
                    if (agent == null || agent == main || !agent.IsHuman || !agent.IsActive()) continue;
                    Team team = agent.Team;
                    if (!IsRealTeam(team) || !team.IsEnemyOf(playerTeam)) continue;
                    if (!outlineFleeing && agent.IsRetreating()) continue;
                    float distSq = agent.Position.DistanceSquared(origin);
                    if (distSq > maxSq) continue;
                    _candidates.Add(new KeyValuePair<float, Agent>(distSq, agent));
                }
                catch (Exception e)
                {
                    if (!_agentErrorLogged)
                    {
                        _agentErrorLogged = true;
                        Log.Debug($"skipped an agent: {e.GetType().Name}: {e.Message}");
                    }
                }
            }

            // Nearest first, so the outline limit keeps the enemies that matter.
            if (maxOutlined > 0 && _candidates.Count > maxOutlined)
            {
                _candidates.Sort((a, b) => a.Key.CompareTo(b.Key));
                _candidates.RemoveRange(maxOutlined, _candidates.Count - maxOutlined);
            }

            _wanted.Clear();
            _wantedSet.Clear();
            foreach (var pair in _candidates)
            {
                Agent agent = pair.Value;
                if (_wantedSet.Add(agent)) _wanted.Add(agent);
                if (!outlineHorses) continue;
                try
                {
                    Agent mount = agent.MountAgent;
                    if (mount != null && mount.IsActive() && _wantedSet.Add(mount)) _wanted.Add(mount);
                }
                catch (Exception) { }
            }

            // Take the outline off anyone who is no longer an enemy in range.
            _toClear.Clear();
            foreach (Agent agent in _outlined.Keys)
                if (!_wantedSet.Contains(agent)) _toClear.Add(agent);
            foreach (Agent agent in _toClear)
                Unoutline(agent);

            foreach (Agent agent in _wanted)
            {
                if (!reassert && _outlined.TryGetValue(agent, out uint current) && current == color) continue;
                SetContour(agent, color, throughWalls);
                _outlined[agent] = color;
            }
        }

        /// <summary>
        /// Agents that belong to no side (arena spectators, townspeople) are on Team.Invalid, which has no mission
        /// behind it: calling IsEnemyOf on it throws a NullReferenceException inside the game.
        /// </summary>
        private static bool IsRealTeam(Team team) => team != null && team.IsValid;

        private void Deactivate()
        {
            if (_outlined.Count > 0) ClearAll();
            if (_wasActive) Log.Debug("highlight off");
            _wasActive = false;
        }

        private void ClearAll()
        {
            _toClear.Clear();
            _toClear.AddRange(_outlined.Keys);
            foreach (Agent agent in _toClear)
                Unoutline(agent);
            _outlined.Clear();
        }

        private void SafeClearAll()
        {
            try { ClearAll(); }
            catch (Exception) { _outlined.Clear(); }
        }

        /// <summary>
        /// Removes only an outline this mod set. If it was on the NPC the player is looking at, the game's own
        /// "you can talk to this person" outline is put back.
        /// </summary>
        private void Unoutline(Agent agent)
        {
            _outlined.Remove(agent);
            SetContour(agent, null, false);
            if (agent != null && agent == _focusedAgent)
                VanillaContour.RestoreFocused(Mission);
        }

        private static void SetContour(Agent agent, uint? color, bool alwaysVisible)
        {
            try
            {
                MBAgentVisuals visuals = agent?.AgentVisuals;
                if (visuals != null && visuals.IsValid())
                    visuals.SetContourColor(color, alwaysVisible);
            }
            catch (Exception)
            {
                // The agent's visuals can already be gone (removed body); nothing to clear then.
            }
        }

        // ------------------------------------------------------------------ mission events

        public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
        {
            base.OnFocusGained(agent, focusableObject, isInteractable);
            _focusedAgent = focusableObject as Agent;
        }

        public override void OnFocusLost(Agent agent, IFocusable focusableObject)
        {
            base.OnFocusLost(agent, focusableObject);
            Agent lost = focusableObject as Agent;
            if (lost == _focusedAgent) _focusedAgent = null;
            // The game clears its focus outline here, which also wipes ours; put ours back right away.
            if (lost != null && _outlined.TryGetValue(lost, out uint color))
                SetContour(lost, color, _lastThroughWalls);
        }

        // A soldier who dies or is knocked out keeps his body in the scene, so take the outline off right away.
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (affectedAgent == _focusedAgent) _focusedAgent = null;
            if (affectedAgent != null && _outlined.Remove(affectedAgent))
                SetContour(affectedAgent, null, false);
        }

        public override void OnEndMissionInternal()
        {
            SafeClearAll();
            base.OnEndMissionInternal();
        }

        public override void OnRemoveBehavior()
        {
            SafeClearAll();
            try { ScreenManager.FocusGained -= OnWindowFocusGained; }
            catch (Exception) { }
            base.OnRemoveBehavior();
        }
    }

    /// <summary>Whether the game window has focus. The game keeps this in a private field; true if it can't be read.</summary>
    internal static class WindowFocus
    {
        private static readonly FieldInfo Field =
            typeof(ScreenManager).GetField("_isWindowFocused", BindingFlags.NonPublic | BindingFlags.Static);

        public static bool IsFocused
        {
            get
            {
                try { return Field == null || (bool)Field.GetValue(null); }
                catch (Exception) { return true; }
            }
        }
    }

    /// <summary>
    /// Talks to the game's MissionAgentContourControllerView, which outlines the NPC you look at in settlements.
    /// Found by name so the mod needs no extra reference; every step fails quietly if a game update changes it.
    /// </summary>
    internal static class VanillaContour
    {
        private const string ViewTypeName = "TaleWorlds.MountAndBlade.View.MissionViews.MissionAgentContourControllerView";
        private static bool _resolved;
        private static FieldInfo _appliedField;
        private static MethodInfo _addMethod;

        public static void RestoreFocused(Mission mission)
        {
            try
            {
                MissionBehavior view = null;
                foreach (MissionBehavior b in mission.MissionBehaviors)
                {
                    if (b != null && b.GetType().FullName == ViewTypeName) { view = b; break; }
                }
                if (view == null) return;
                if (!_resolved)
                {
                    _resolved = true;
                    Type t = view.GetType();
                    _appliedField = t.GetField("_isContourAppliedToFocusedAgent", BindingFlags.NonPublic | BindingFlags.Instance);
                    _addMethod = t.GetMethod("AddContourToFocusedAgent", BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                }
                if (_appliedField == null || _addMethod == null) return;
                _appliedField.SetValue(view, false);
                _addMethod.Invoke(view, null);
            }
            catch (Exception)
            {
                // Only cosmetic: the game puts the outline back the next time you look at the NPC.
            }
        }
    }
}
