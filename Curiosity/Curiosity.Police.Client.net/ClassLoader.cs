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
        public static Dictionary<int, Func<bool>> RuralCallOuts = new Dictionary<int, Func<bool>>();
        public static Dictionary<int, Func<bool>> CountryCallOuts = new Dictionary<int, Func<bool>>();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            Classes.Player.PlayerInformation.Init();
            Environment.Job.DutyManager.Init();
            Environment.Tasks.CalloutHandler.Init();

            Environment.Vehicle.Sirens.Init();

            // Developer
            Classes.Developer.Init();

            CityCallOuts.Add(1, Environment.Tasks.Callouts.City.Shops.ClintonAve.Init);
            CityCallOuts.Add(2, Environment.Tasks.Callouts.City.Shops.SanAndreasAve.Init);
            CityCallOuts.Add(3, Environment.Tasks.Callouts.City.Shops.ProsperityStreet.Init);
            CityCallOuts.Add(4, Environment.Tasks.Callouts.City.Shops.LittleSeoul.Init);
            CityCallOuts.Add(5, Environment.Tasks.Callouts.City.Shops.Strawberry.Init);
            CityCallOuts.Add(6, Environment.Tasks.Callouts.City.Shops.DavisAve.Init);
            CityCallOuts.Add(7, Environment.Tasks.Callouts.City.Shops.MurrietaHeights.Init);
            CityCallOuts.Add(8, Environment.Tasks.Callouts.City.Shops.MirrorPark.Init);

            RuralCallOuts.Add(9, Environment.Tasks.Callouts.Rural.Shops.BanhamCanyonRobsLiquor.Init);
            RuralCallOuts.Add(10, Environment.Tasks.Callouts.Rural.Shops.BanhamCanyonTwentyFour.Init);
            RuralCallOuts.Add(11, Environment.Tasks.Callouts.Rural.Shops.Chumash.Init);
            RuralCallOuts.Add(12, Environment.Tasks.Callouts.Rural.Shops.RichmanGlen.Init);
            RuralCallOuts.Add(13, Environment.Tasks.Callouts.Rural.Shops.TataviamMountains.Init);

            CountryCallOuts.Add(14, Environment.Tasks.Callouts.Country.Shops.GrandSenoraDesertScoops.Init);
            CountryCallOuts.Add(15, Environment.Tasks.Callouts.Country.Shops.GrandSenoraDesertTwentyFour.Init);
            CountryCallOuts.Add(16, Environment.Tasks.Callouts.Country.Shops.Grapeseed.Init);
            CountryCallOuts.Add(17, Environment.Tasks.Callouts.Country.Shops.Harmony.Init);
            CountryCallOuts.Add(18, Environment.Tasks.Callouts.Country.Shops.MountChiliad.Init);
            CountryCallOuts.Add(19, Environment.Tasks.Callouts.Country.Shops.SandyShoresLiquorAce.Init);
            CountryCallOuts.Add(20, Environment.Tasks.Callouts.Country.Shops.SandyShoresTwentyFour.Init);

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