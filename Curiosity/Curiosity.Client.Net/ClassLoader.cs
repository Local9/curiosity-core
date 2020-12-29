using Curiosity.Client.net.Classes.Actions;
using Curiosity.Client.net.Classes.Actions.Emotes;
using Curiosity.Client.net.Classes.Environment;
using Curiosity.Client.net.Classes.Environment.IPL;
using Curiosity.Client.net.Classes.Environment.UI;
using Curiosity.Client.net.Classes.PlayerClasses;

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
            Scaleforms.Init();
            ChatCommands.Init();
            // Emotes
            Pointing.Init();
            // UI
            HideReticle.Init();
            PlayerOverheadMarkers.Init();
            Notifications.Init();
            PlayerBlips.Init();
            PlayerNames.Init();
            // Environment
            AfkKick.Init();
            WarpPoints.Init();
            Voip.Init();
            Vehicles.Init();
            SpawnManagement.Init();
            PlayerInformation.Init();
            WantedLevels.Init();
            Weapons.Init();
            DevCommands.Init();
            IplLoader.Init();
            WorldScenarios.Init();
        }
    }
}