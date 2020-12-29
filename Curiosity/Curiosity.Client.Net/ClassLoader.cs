using Curiosity.Client.net.Classes.Actions;
using Curiosity.Client.net.Classes.Actions.Emotes;
using Curiosity.Client.net.Classes.Environment;
using Curiosity.Client.net.Classes.Environment.IPL;
using Curiosity.Client.net.Classes.Environment.PDA;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Client.net.ClientExports;

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

            Queue.Init();

            // Classes.RandomKiller.Init();

            // Addon stuff
            // RenderTargets.Init();
            // DuiHandler.Init();

            Scaleforms.Init();
            ChatCommands.Init();

            // IPLs
            // Classes.Environment.IPL.NightclubBase.Init();
            Classes.Environment.IPL.Nightclub.IplManager.Init();
            // Classes.Environment.IPL.Nightclub.EffectsManager.Init();

            // Emotes
            Pointing.Init();

            // UI
            // ButtonInstructions.Init(); // To be tested
            // CinematicMode.Init();
            HideReticle.Init();
            // Location.Init();
            PlayerOverheadMarkers.Init();
            // PlayerNames.Init();
            // PlayerBlips.Init();
            // Scoreboard.Init();
            Notifications.Init();

            // MENU
            // NuiEventHandler.Init();

            // Environment
            AfkKick.Init();
            WarpPoints.Init();
            Voip.Init();
            // VideoLoader.Init();
            Vehicles.Init();
            // Sit.Init();
            SpawnManagement.Init();
            Classes.Environment.PedClasses.PedHandler.Init();
            // Skills.Init(); Not used
            // Bank.Init();
            PlayerInformation.Init();
            WantedLevels.Init();
            Weapons.Init();

            MarqueeMessages.Init();
            ChatService.Init();
            DevCommands.Init();
            IplLoader.Init();
            // PdaEvents.Init();
            WorldScenarios.Init();
            SeasonSync.Init();

            NotificationExport.Init();
        }
    }
}