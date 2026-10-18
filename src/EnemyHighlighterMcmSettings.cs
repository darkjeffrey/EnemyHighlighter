using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

namespace EnemyHighlighter
{
    /// <summary>The "Enemy Highlighter" page in Mod Options (MCM). Saved to Documents\Mount and Blade II Bannerlord\Configs\ModSettings.</summary>
    public sealed class EnemyHighlighterMcmSettings : AttributeGlobalSettings<EnemyHighlighterMcmSettings>
    {
        public override string Id => "EnemyHighlighter";
        // MCM shows DisplayName as-is (it doesn't translate it like setting names and hints), so resolve it here.
        public override string DisplayName => Txt.Resolve("{=ehl_mcm_title}Enemy Highlighter");
        public override string FolderName => "EnemyHighlighter";
        public override string FormatType => "json2";

        // ---------------- General ----------------
        private const string General = "{=ehl_mcm_group_general}General";

        [SettingPropertyBool("{=ehl_mcm_Enabled}Enable", Order = 0, RequireRestart = false,
            HintText = "{=ehl_mcm_Enabled_hint}Turn the mod on or off.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public bool Enabled { get; set; } = true;

        [SettingPropertyDropdown("{=ehl_mcm_HighlightKey}Highlight key", Order = 1, RequireRestart = false,
            HintText = "{=ehl_mcm_HighlightKey_hint}The key that shows the enemy outlines. Left Alt is also the game's Show Indicators key, so both show together. Careful when holding Left Alt: Alt+F4 closes the game, and F4 is a battle order key. A key the game already uses (W, Space, number keys...) will also do its normal job each time you press it.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public Dropdown<string> HighlightKey { get; set; } = new Dropdown<string>(KeyChoices.Names, KeyChoices.DefaultIndex);

        [SettingPropertyBool("{=ehl_mcm_ToggleMode}Toggle mode", Order = 2, RequireRestart = false,
            HintText = "{=ehl_mcm_ToggleMode_hint}On: press the key once to turn the outlines on and again to turn them off. Off: the outlines show only while you hold the key.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public bool ToggleMode { get; set; } = false;

        [SettingPropertyBool("{=ehl_mcm_KeepToggleBetweenScenes}Keep toggle between scenes", Order = 3, RequireRestart = false,
            HintText = "{=ehl_mcm_KeepToggleBetweenScenes_hint}In toggle mode: if the outlines are on when you leave a scene (walking into the tavern, the next battle), they start on in the next one. Off: every scene starts with the outlines off.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public bool KeepToggleBetweenScenes { get; set; } = false;

        [SettingPropertyBool("{=ehl_mcm_WarnKeyConflicts}Warn about key conflicts", Order = 5, RequireRestart = false,
            HintText = "{=ehl_mcm_WarnKeyConflicts_hint}Once per game session, show a message if the highlight key is also bound to one of the game's controls (Show Indicators on Left Alt doesn't count).")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public bool WarnKeyConflicts { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_ShowToggleMessage}Message when toggled", Order = 4, RequireRestart = false,
            HintText = "{=ehl_mcm_ShowToggleMessage_hint}In toggle mode, show a short line in the message log each time the outlines turn on or off.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public bool ShowToggleMessage { get; set; } = true;

        [SettingPropertyInteger("{=ehl_mcm_MaxDistance}Maximum distance", 5, 300, "0 m", Order = 6, RequireRestart = false,
            HintText = "{=ehl_mcm_MaxDistance_hint}Only enemies closer than this get an outline, so far-away troops don't clutter the screen. Measured from you, or from the camera once you're knocked out.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public int MaxDistance { get; set; } = 60;

        [SettingPropertyInteger("{=ehl_mcm_MaxOutlined}Most outlines at once", 0, 500, "0", Order = 7, RequireRestart = false,
            HintText = "{=ehl_mcm_MaxOutlined_hint}Only the nearest this many enemies get an outline. Keeps big battles smooth at long distances. 0 = no limit.")]
        [SettingPropertyGroup(General, GroupOrder = 0)]
        public int MaxOutlined { get; set; } = 100;

        // ---------------- Look ----------------
        private const string Look = "{=ehl_mcm_group_look}Look";

        [SettingPropertyDropdown("{=ehl_mcm_OutlineColor}Outline colour", Order = 0, RequireRestart = false,
            HintText = "{=ehl_mcm_OutlineColor_hint}The colour of the enemy outline. The two colour-blind colours stay distinct for red-green colour blindness.")]
        [SettingPropertyGroup(Look, GroupOrder = 1)]
        public Dropdown<ColorChoice> OutlineColor { get; set; } = new Dropdown<ColorChoice>(ColorChoice.All, 0);

        [SettingPropertyBool("{=ehl_mcm_ThroughWalls}Show through walls", Order = 1, RequireRestart = false,
            HintText = "{=ehl_mcm_ThroughWalls_hint}On: the outline also shows when an enemy is behind a wall, a tree or other soldiers. Off: only the parts of him you can see get an outline.")]
        [SettingPropertyGroup(Look, GroupOrder = 1)]
        public bool ThroughWalls { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_OutlineFleeing}Outline fleeing enemies", Order = 2, RequireRestart = false,
            HintText = "{=ehl_mcm_OutlineFleeing_hint}Off: enemies who are running away get no outline, so you only see the ones still fighting.")]
        [SettingPropertyGroup(Look, GroupOrder = 1)]
        public bool OutlineFleeing { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_OutlineHorses}Outline enemy horses", Order = 3, RequireRestart = false,
            HintText = "{=ehl_mcm_OutlineHorses_hint}Also outline the horse of an enemy rider. Riderless horses never get an outline.")]
        [SettingPropertyGroup(Look, GroupOrder = 1)]
        public bool OutlineHorses { get; set; } = false;

        // ---------------- Where ----------------
        private const string Where = "{=ehl_mcm_group_where}Where it works";

        [SettingPropertyBool("{=ehl_mcm_InBattles}Battles and sieges", Order = 0, RequireRestart = false,
            HintText = "{=ehl_mcm_InBattles_hint}Field battles, sieges, village raids and custom battles.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool InBattles { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_InHideouts}Hideouts", Order = 1, RequireRestart = false,
            HintText = "{=ehl_mcm_InHideouts_hint}Hideout raids, including the fight with the bandit leader.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool InHideouts { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_InArenas}Arenas and tournaments", Order = 2, RequireRestart = false,
            HintText = "{=ehl_mcm_InArenas_hint}Arena practice fights, duels and tournament rounds.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool InArenas { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_InSettlements}Towns, castles and villages", Order = 3, RequireRestart = false,
            HintText = "{=ehl_mcm_InSettlements_hint}Fights inside settlements: alley fights, prison breaks, lord's hall fights and anyone hostile to you while you walk around.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool InSettlements { get; set; } = true;

        [SettingPropertyBool("{=ehl_mcm_AllowInStealth}Stealth missions", Order = 4, RequireRestart = false,
            HintText = "{=ehl_mcm_AllowInStealth_hint}The disguise mission, prison breaks and the sneaking part of a hideout attack. Off by default, because outlines through walls make sneaking too easy.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool AllowInStealth { get; set; } = false;

        [SettingPropertyBool("{=ehl_mcm_HideInCutscenes}Hide during cutscenes", Order = 5, RequireRestart = false,
            HintText = "{=ehl_mcm_HideInCutscenes_hint}No outlines while a cutscene plays (such as the bandit leader's entrance in a hideout) or in photo mode. They come back on their own afterwards.")]
        [SettingPropertyGroup(Where, GroupOrder = 2)]
        public bool HideInCutscenes { get; set; } = true;

        // ---------------- Testing ----------------
        private const string Testing = "{=ehl_mcm_group_testing}Testing";

        [SettingPropertyBool("{=ehl_mcm_DebugMessages}Debug messages", Order = 0, RequireRestart = false,
            HintText = "{=ehl_mcm_DebugMessages_hint}Show chat messages about what the mod is doing: the kind of scene it detected, and how many enemies got an outline. Useful for checking the mod works; turn it off for normal play.")]
        [SettingPropertyGroup(Testing, GroupOrder = 3)]
        public bool DebugMessages { get; set; } = false;
    }
}
