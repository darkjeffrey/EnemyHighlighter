using System;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Missions.MissionLogics.Hideout;
using SandBox.Missions.MissionLogics.Towns;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;

namespace EnemyHighlighter
{
    public class SubModule : MBSubModuleBase
    {
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            try
            {
                // Single player only. Custom battles are included on purpose, so no Campaign.Current check.
                if (mission == null || GameNetwork.IsMultiplayer) return;
                // Always added: Enable and the per-scene options are read every tick, so they can be changed in
                // Mod Options without leaving the scene.
                mission.AddMissionBehavior(new EnemyHighlightLogic(Classify(mission), IsStealthScene(mission)));
            }
            catch (Exception e)
            {
                Log.Error("OnMissionBehaviorInitialize", e);
            }
        }

        /// <summary>
        /// Which kind of scene this is, from the vanilla behaviors it was built with. The order matters: the arena
        /// and hideout ambush missions also have the settlement handlers.
        /// </summary>
        private static SceneKind Classify(Mission mission)
        {
            if (mission.GetMissionBehavior<ArenaPracticeFightMissionController>() != null
                || mission.GetMissionBehavior<ArenaDuelMissionController>() != null
                || mission.GetMissionBehavior<TournamentFightMissionController>() != null
                || mission.GetMissionBehavior<TournamentArcheryMissionController>() != null
                || mission.GetMissionBehavior<TournamentJoustingMissionController>() != null)
                return SceneKind.Arena;

            if (mission.GetMissionBehavior<HideoutMissionController>() != null
                || mission.GetMissionBehavior<HideoutAmbushMissionController>() != null
                || mission.GetMissionBehavior<HideoutPhasedMissionController>() != null)
                return SceneKind.Hideout;

            if (mission.GetMissionBehavior<MissionAgentHandler>() != null
                || mission.GetMissionBehavior<MissionFightHandler>() != null
                || mission.GetMissionBehavior<AlleyFightMissionHandler>() != null
                || mission.GetMissionBehavior<PrisonBreakMissionController>() != null
                || mission.GetMissionBehavior<LordsHallFightMissionController>() != null)
                return SceneKind.Settlement;

            return SceneKind.Battle;
        }

        /// <summary>
        /// Missions that are all about sneaking: the disguise mission and the prison break. The hideout ambush
        /// only sneaks at the start; that part is caught at run time from the mission mode (Stealth).
        /// </summary>
        private static bool IsStealthScene(Mission mission)
        {
            return mission.GetMissionBehavior<DisguiseMissionLogic>() != null
                || mission.GetMissionBehavior<StealthMissionController>() != null
                || mission.GetMissionBehavior<PrisonBreakMissionController>() != null;
        }
    }

    public enum SceneKind
    {
        Battle,
        Hideout,
        Arena,
        Settlement,
    }
}
