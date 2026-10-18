using System;
using TaleWorlds.InputSystem;

namespace EnemyHighlighter
{
    /// <summary>
    /// Read-only view of the current settings. Values come from the MCM page (EnemyHighlighterMcmSettings);
    /// the defaults there are used if MCM isn't loaded yet.
    /// </summary>
    public static class EhSettings
    {
        private static readonly EnemyHighlighterMcmSettings Defaults = new EnemyHighlighterMcmSettings();

        private static EnemyHighlighterMcmSettings S
        {
            get
            {
                try { return EnemyHighlighterMcmSettings.Instance ?? Defaults; }
                catch (Exception) { return Defaults; }
            }
        }

        public static bool Enabled => S.Enabled;
        public static bool ToggleMode => S.ToggleMode;
        public static bool ShowToggleMessage => S.ShowToggleMessage;
        public static float MaxDistance => Math.Max(5, Math.Min(300, S.MaxDistance));
        public static int MaxOutlined => Math.Max(0, Math.Min(500, S.MaxOutlined));
        public static bool KeepToggleBetweenScenes => S.KeepToggleBetweenScenes;
        public static bool WarnKeyConflicts => S.WarnKeyConflicts;
        public static bool AllowInStealth => S.AllowInStealth;
        public static bool HideInCutscenes => S.HideInCutscenes;
        public static bool ThroughWalls => S.ThroughWalls;
        public static bool OutlineFleeing => S.OutlineFleeing;
        public static bool OutlineHorses => S.OutlineHorses;
        public static bool DebugMessages => S.DebugMessages;

        public static InputKey HighlightKey
        {
            get
            {
                try { return KeyChoices.Parse(S.HighlightKey?.SelectedValue); }
                catch (Exception) { return KeyChoices.DefaultKey; }
            }
        }

        public static uint OutlineColor
        {
            get
            {
                try { return (S.OutlineColor?.SelectedValue ?? ColorChoice.All[0]).Value; }
                catch (Exception) { return ColorChoice.All[0].Value; }
            }
        }

        public static bool IsAllowedIn(SceneKind kind)
        {
            switch (kind)
            {
                case SceneKind.Hideout: return S.InHideouts;
                case SceneKind.Arena: return S.InArenas;
                case SceneKind.Settlement: return S.InSettlements;
                default: return S.InBattles;
            }
        }
    }
}
