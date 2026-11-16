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

        private static EnemyHighlighterMcmSettings Current
        {
            get
            {
                try { return EnemyHighlighterMcmSettings.Instance ?? Defaults; }
                catch (Exception) { return Defaults; }
            }
        }

        public static bool Enabled => Current.Enabled;
        public static bool ToggleMode => Current.ToggleMode;
        public static bool ShowToggleMessage => Current.ShowToggleMessage;
        public static float MaxDistance => Math.Max(5, Math.Min(300, Current.MaxDistance));
        public static int MaxOutlined => Math.Max(0, Math.Min(500, Current.MaxOutlined));
        public static bool KeepToggleBetweenScenes => Current.KeepToggleBetweenScenes;
        public static bool WarnKeyConflicts => Current.WarnKeyConflicts;
        public static bool AllowInStealth => Current.AllowInStealth;
        public static bool HideInCutscenes => Current.HideInCutscenes;
        public static bool ThroughWalls => Current.ThroughWalls;
        public static bool OutlineFleeing => Current.OutlineFleeing;
        public static bool OutlineHorses => Current.OutlineHorses;
        public static bool DebugMessages => Current.DebugMessages;

        public static InputKey HighlightKey
        {
            get
            {
                try { return KeyChoices.Parse(Current.HighlightKey?.SelectedValue); }
                catch (Exception) { return KeyChoices.DefaultKey; }
            }
        }

        public static uint OutlineColor
        {
            get
            {
                try { return (Current.OutlineColor?.SelectedValue ?? ColorChoice.All[0]).Value; }
                catch (Exception) { return ColorChoice.All[0].Value; }
            }
        }

        public static bool IsAllowedIn(SceneKind kind)
        {
            switch (kind)
            {
                case SceneKind.Hideout: return Current.InHideouts;
                case SceneKind.Arena: return Current.InArenas;
                case SceneKind.Settlement: return Current.InSettlements;
                default: return Current.InBattles;
            }
        }
    }
}
