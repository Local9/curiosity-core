using Curiosity.Shared.Client.net;
using System.Collections.Generic;
using System;

namespace Curiosity.Police.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        public static Dictionary<int, Func<bool>> CityCallOuts = new Dictionary<int, Func<bool>>();
        public static Dictionary<int, Func<bool>> CountryCallOuts = new Dictionary<int, Func<bool>>();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            Classes.Player.PlayerInformation.Init();
            Environment.Job.DutyManager.Init();

            // Developer
            Classes.Developer.Init();

            CityCallOuts.Add(1, Environment.Tasks.Callouts.City.Downtown.ClintonAveShop.Init);
            CityCallOuts.Add(2, Environment.Tasks.Callouts.City.VespucciCanals.SanAndreasAveShop.Init);
            CityCallOuts.Add(3, Environment.Tasks.Callouts.City.Morningwood.ProsperityStreetShop.Init);
            CityCallOuts.Add(4, Environment.Tasks.Callouts.City.Palomino.LittleSeoulShop.Init);
            CityCallOuts.Add(5, Environment.Tasks.Callouts.City.ElginAve.StrawberryShop.Init);
            CityCallOuts.Add(6, Environment.Tasks.Callouts.City.DavisAve.DavisAveShop.Init);
            CityCallOuts.Add(7, Environment.Tasks.Callouts.City.VespucciBlvd.MurrietaHeightsShop.Init);
            CityCallOuts.Add(8, Environment.Tasks.Callouts.City.WestMirrorDrive.MirrorParkShop.Init);

            /// INSPIRATION!
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

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}