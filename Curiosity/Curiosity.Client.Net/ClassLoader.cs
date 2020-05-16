using Curiosity.Client.net.Classes.Actions;
using Curiosity.Client.net.Classes.Actions.Emotes;
using Curiosity.Client.net.Classes.Environment;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Client.net.Helpers;
using Curiosity.Client.net.Helpers.Dictionary;

namespace Curiosity.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        public static void Init()
        {
            // https://github.com/GroovyGiantPanda/FiveMRpServerResources/tree/master/src/FiveM/RPClient/Classes

            Classes.Player.Queue.Init();

            // Addon stuff
            RenderTargets.Init();
            DuiHandler.Init();
            // WayPoints.Init();
            Scaleforms.Init();
            ChatCommands.Init();

            // IPLs
            Classes.Environment.IPL.NightclubBase.Init();
            Classes.Environment.IPL.Nightclub.IplManager.Init();
            Classes.Environment.IPL.Nightclub.EffectsManager.Init();

            // Emotes
            //EmotesManager.Init();
            //HandsUp.Init();
            Pointing.Init();

            // UI
            //ControlCodeTester.Init();
            //Radar.Init();
            ButtonInstructions.Init(); // To be tested
            CinematicMode.Init();
            HideReticle.Init();
            Location.Init();
            PlayerOverheadMarkers.Init();
            PlayerNames.Init();
            PlayerBlips.Init();
            // Speedometer.Init();
            // Speedometer3D.Init();
            //VehicleLoadoutPresets.Init();
            Scoreboard.Init();
            // WorldTime.Init();
            Notifications.Init();

            // MENU
            //MenuGlobals.Init();
            ////VehicleMenuTest.Init();
            //SkillsMenu.Init();
            //CharacterMenuTest.Init();
            //InteractionListMenu.Init();
            //NuiMenu.Init();
            NuiEventHandler.Init();
            //PlayerActionsMenu.Init();

            // Environment
            //Poi.Init();
            AfkKick.Init();
            //ManipulateObject.Init();
            //NoClip.Init();
            //MarkerHandler.Init();
            //Pvp.Init();
            WarpPoints.Init();
            Voip.Init();
            VideoLoader.Init();
            //EmergencyServices.Init();
            Vehicles.Init();
            // DeleteProps.Init();
            Classes.Player.Sit.Init();
            SpawnManagement.Init();
            // InstancingChecker.Init();
            Classes.Environment.PedClasses.PedHandler.Init();

            // EMS
            //EMS.Init();

            // Police
            //Sirens.Init();
            //WeaponStash.Init();
            //Arrest.Init(); 
            //CivilianCarSirenLights.Init();
            //CustomizationCommands.Init();
            //Helicopter.Init();
            //SkinLoadoutPresets.Init();
            //Slimjim.Init();
            //SpikeStrip.Init();
            //CellDoors.Init();
            //Tackle.Init();
            //PoliceCharacterMenu.Init();
            //PoliceVehicleMenu.Init();
            //DutyManager.Init();

            // Jobs
            //TrainManager.Init();
            //Fishing.Init();
            //Hunting.Init();
            //Garages.Init();

            // Player
            //GunShotResidueManager.Init();
            //PedDamage.Init(); // In but off
            //PrisonSentence.Init();
            //DeathHandler.Init();
            //WeaponUnholsterHandler.Init();
            Classes.Player.Skills.Init();
            Bank.Init();
            Classes.Player.PlayerInformation.Init();
            WantedLevels.Init();
            // MugshotCreator.Init();
            //Classes.Player.Inventory.Init();
            Classes.Player.Weapons.Init();
            // Classes.Player.Creation.Init();

            // Vehicles
            //Vehicles.Init();
            //CarHud.Init();
            //BannedMilitaryVehicles.Init();
            //LockManager.Init();
            //Lockpicking.Init();
            //RandomCarLocks.Init();
            //EmotesManager.Init();
            //Stay.Init();

            Classes.Environment.Trains.Init();

            MarqueeMessages.Init();
            ChatService.Init();
            DevCommands.Init();
        }
    }
}