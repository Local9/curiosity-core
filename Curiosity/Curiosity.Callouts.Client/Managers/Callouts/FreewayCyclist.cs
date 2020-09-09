using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class FreewayCyclist : Callout
    {
        private CalloutMessage calloutMessage = new CalloutMessage();
        public FreewayCyclist(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);
        private Vector3[] coordinates = new Vector3[] { new Vector3(1308.065f, 581.2718f, 79.78977f), new Vector3(1594.56f, 1009.64f, 78.95673f), new Vector3(1689.932f, 1352.383f, 87.02444f), new Vector3(1878.03f, 2336.891f, 55.68655f), new Vector3(2061.051f, 2644.353f, 52.16157f), new Vector3(2360.496f, 2856.932f, 40.1833f), new Vector3(2539.596f, 3042.605f, 42.92778f), new Vector3(2946.281f, 3813.16f, 52.26584f), new Vector3(2795.884f, 446.548f, 48.0796f), new Vector3(2649.814f, 4928.652f, 44.39455f), new Vector3(2331.14f, 5905.173f, 47.67876f), new Vector3(1436.997f, 6474.696f, 20.40421f), new Vector3(777.1246f, 6513.02f, 24.64042f), new Vector3(-589.123f, 5663.769f, 38.00635f), new Vector3(-1529.918f, 4981.798f, 62.0877228f), new Vector3(-2329.675f, 4112.701f, 35.33438f), new Vector3(-589.123f, 5663.769f, 38.00635f), new Vector3(-1529.918f, 4981.798f, 62.08722f), new Vector3(-2329.675f, 4112.701f, 35.33438f), new Vector3(-2620.146f, 2824.454f, 16.38638f), new Vector3(-3039.727f, 1872.351f, 29.84845f), new Vector3(-3128.839f, 835.1783f, 16.17631f), new Vector3(-2539.692f, -185.8579f, 19.42014f), new Vector3(-1842.228f, -595.9995f, 11.09579f) };

        public static bool HasPedBeenArrested;

        Ped Cyclist;
        Vehicle Bicycle;
        Vector3 SpawnLocation;

        List<VehicleHash> bicycles = new List<VehicleHash>
        {
            VehicleHash.Bmx,
            VehicleHash.TriBike,
            VehicleHash.TriBike2,
            VehicleHash.TriBike3,
            VehicleHash.Scorcher,
            VehicleHash.Cruiser
        };

        internal async override void Prepare()
        {
            base.Prepare();
            bool flag;

            Vector3 vector3;
            do
            {
                SpawnLocation = this.coordinates[Utility.RANDOM.Next(0, (int)this.coordinates.Length)];
                flag = (!Game.PlayerPed.IsInRangeOf(SpawnLocation, 1100f) ? false : !Game.PlayerPed.IsInRangeOf(SpawnLocation, 200f));

                await BaseScript.Delay(10);
            }
            while (flag);

            progress = 2;
            UiTools.Dispatch("Cyclist on Freeway", "Reports of a cyclyst on the freeway, GPS has been updated.");

#if DEBUG
            UiTools.Dispatch("Cyclist on Freeway", $"{SpawnLocation}");
#endif

            Model model = bicycles.Random();
            Cyclist = await Ped.SpawnRandom(SpawnLocation, false);
            Cyclist.IsImportant = true;
            Cyclist.IsSuspect = true;

            Bicycle = await Vehicle.Spawn(model, SpawnLocation);
            Bicycle.IsMission = true;

            Cyclist.Task.WarpIntoVehicle(Bicycle.Fx, VehicleSeat.Driver);
            Cyclist.Task.CruiseWithVehicle(Bicycle.Fx, 60f, (int)Collections.CombinedVehicleDrivingFlags.Normal);

            Blip blip = Cyclist.AttachBlip(BlipColor.Blue, true);
            blip.Sprite = (BlipSprite)143;
            blip.Name = "Cyclist";

            RegisterPed(Cyclist);
            RegisterVehicle(Bicycle);

            base.IsSetup = true;
        }

        internal override void End(bool forcefully = false, CalloutMessage cm = null)
        {
            cm = calloutMessage;
            base.End(forcefully);
        }

        internal async override void Tick()
        {
            switch (progress)
            {

            }
#if DEBUG
            if (PlayerManager.IsDeveloper && Game.PlayerPed.Position.Distance(Cyclist.Position) > 50f && PlayerManager.IsDeveloperUIActive)
                Screen.ShowSubtitle($"Dis: {Game.PlayerPed.Position.Distance(Cyclist.Position)}");
#endif
        }
    }
}
