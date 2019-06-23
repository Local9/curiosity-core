//using FamilyRP.Roleplay.Client.Classes;
//using FamilyRP.Roleplay.Client.Classes.Actions;
//using FamilyRP.Roleplay.Client.Classes.Actions.Emotes;
//using FamilyRP.Roleplay.Client.Classes.Actions.EMS;
//using FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police;
//using FamilyRP.Roleplay.Client.Classes.Environment;
//using FamilyRP.Roleplay.Client.Classes.Environment.Controls;
//using FamilyRP.Roleplay.Client.Classes.Environment.DevTools;
//using FamilyRP.Roleplay.Client.Classes.Environment.UI;
//using FamilyRP.Roleplay.Client.Classes.Environment.UI.Police;
//using FamilyRP.Roleplay.Client.Classes.Jobs;
//using FamilyRP.Roleplay.Client.Classes.Jobs.Police;
//using FamilyRP.Roleplay.Client.Classes.Jobs.Police.Helicopter;
//using FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle;
//using FamilyRP.Roleplay.Client.Classes.Jobs.Transportation.Trains;
//using FamilyRP.Roleplay.Client.Classes.Player;
//using FamilyRP.Roleplay.Client.Classes.Vehicle;
using Curiosity.Client.net.Classes.Actions;
using Curiosity.Client.net.Classes.Actions.Emotes;
using Curiosity.Client.net.Classes.Environment;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Client.net.Classes.Player;
using Curiosity.Client.net.Classes.Vehicle;
using Curiosity.Client.net.Helpers;
using Curiosity.Client.net.Helpers.Dictionary;
using Curiosity.Shared.Client.net;

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
            Log.Verbose("Entering ClassLoader Init");

            // https://github.com/GroovyGiantPanda/FiveMRpServerResources/tree/master/src/FiveM/RPClient/Classes

            Queue.Init();

            // Addon stuff
            RenderTargets.Init();
            DuiHandler.Init();
            // WayPoints.Init();
            Scaleforms.Init();
            ChatCommands.Init();

            // IPLs
            // Classes.Environment.IPL.Nightclub.Init();

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
            Speedometer.Init();
            Speedometer3D.Init();
            //VehicleLoadoutPresets.Init();
            Scoreboard.Init();
            WorldTime.Init();
            Notifications.Init();
            // MobilePhone.Init();

            // MENU
            //MenuGlobals.Init();
            ////VehicleMenuTest.Init();
            //SkillsMenu.Init();
            //CharacterMenuTest.Init();
            //InteractionListMenu.Init();
            //NuiMenu.Init();
            NuiEventHandler.Init();
            //PlayerActionsMenu.Init();

            // MENU 2.0
            Classes.Menus.MenuBase.Init();
            // Classes.Menus.Inventory.Init();
            Classes.Menus.PlayerMenu.Init();
            // ONLINE PLAYER MENU ITEMS
            Classes.Menus.OnlinePlayers.Init();
            Classes.Menus.PlayerInteractions.ReportInteraction.Init();
            Classes.Menus.PlayerInteractions.KickInteraction.Init();
            Classes.Menus.PlayerInteractions.BanInteraction.Init();
            // Additional Items
            Classes.Menus.VehicleMenu.Init();

            // Environment
            //Poi.Init();
            AfkKick.Init();
            //ManipulateObject.Init();
            //NoClip.Init();
            MarkerHandler.Init();
            //Pvp.Init();
            WarpPoints.Init();
            Voip.Init();
            AquaticSpawner.Init();
            VideoLoader.Init();
            //EmergencyServices.Init();
            Vehicles.Init();
            // DeleteProps.Init();
            Sit.Init();
            Birds.Init();
            SpawnManagement.Init();
            // InstancingChecker.Init();

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
            Skills.Init();
            Bank.Init();
            PlayerInformation.Init();
            WantedLevels.Init();
            // MugshotCreator.Init();
            Classes.Player.Inventory.Init();
            Weapons.Init();

            // Vehicles
            //Vehicles.Init();
            //CarHud.Init();
            //BannedMilitaryVehicles.Init();
            BrakeSignals.Init();
            CruiseControl.Init();
            VehicleDamage.Init();
            EngineManager.Init();
            //VehicleWindowManager.Init();
            //DoorManager.Init();
            FuelManager.Init();
            //LockManager.Init();
            //Lockpicking.Init();
            //RandomCarLocks.Init();
            //EmotesManager.Init();
            //Stay.Init();
            DisableAirControls.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}