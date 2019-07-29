using Curiosity.Shared.Client.net;
using System;

namespace Curiosity.Menus.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            // MENU 2.0
            Classes.Menus.MenuBase.Init();
            //// Classes.Menus.Inventory.Init();
            //Classes.Menus.PlayerMenu.Init();
            ////Classes.Menus.PlayerCreator.PlayerCreatorMenu.Init();
            ////Classes.Menus.PlayerCreator.PlayerOverlays.Init();
            ////Classes.Menus.PlayerCreator.PlayerComponents.Init();
            ////Classes.Menus.PlayerCreator.PlayerProps.Init();
            ////Classes.Menus.PlayerCreator.PlayerSave.Init();
            //// ONLINE PLAYER MENU ITEMS
            //Classes.Menus.OnlinePlayers.Init();
            //Classes.Menus.PlayerInteractions.ReportInteraction.Init();
            //Classes.Menus.PlayerInteractions.KickInteraction.Init();
            //Classes.Menus.PlayerInteractions.BanInteraction.Init();
            //// Additional Items
            //Classes.Menus.VehicleMenu.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}