using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers.Callouts
{
    internal class ParkingViolation : Callout
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private CalloutMessage calloutMessage = new CalloutMessage();

        Vehicle parkedVehicle;
        Blip blip;

        List<VehicleHash> vehicleHashes = new List<VehicleHash>()
        {
            VehicleHash.Oracle2,
            VehicleHash.Panto,
            VehicleHash.Sandking,
            VehicleHash.SlamVan,
            VehicleHash.Adder,
            VehicleHash.Faggio,
            VehicleHash.Issi2,
            VehicleHash.Kuruma,
            VehicleHash.F620,
            VehicleHash.Dukes,
            VehicleHash.Baller,
            VehicleHash.Boxville,
            VehicleHash.Rumpo
        };

        public ParkingViolation(Player primaryPlayer) : base(primaryPlayer) => Players.Add(primaryPlayer);

        internal async override void Prepare()
        {
            base.Prepare();

            progress = 1;

            parkedVehicle = await Vehicle.Spawn(vehicleHashes.Random(),
                Players[0].Character.Position.AroundStreet(250f, 600f));

            parkedVehicle.IsMission = true;
            parkedVehicle.IsTowable = true;

            RegisterVehicle(parkedVehicle);

            parkedVehicle.Fx.LockStatus = VehicleLockStatus.Locked;

            int X = Utility.RANDOM.Next(20, 40);
            int Y = Utility.RANDOM.Next(20, 40);

            Vector3 location = parkedVehicle.Fx.GetOffsetPosition(new Vector3(X, Y, 0));

            blip = World.CreateBlip(location);
            blip.Alpha = 120;
            blip.Color = BlipColor.Yellow;
            blip.Sprite = BlipSprite.BigCircle;
            blip.IsShortRange = true;
            blip.Scale = 1f;

            // Get the localized vehicle name, if it's "NULL" (no label found) then use the "properCasedModelName" created above.
            string vehName = GetVehDisplayNameFromModel(parkedVehicle.Fx.Model.Hash);

            string color = $"{parkedVehicle.Fx.Mods.PrimaryColor}~n~{parkedVehicle.Fx.Mods.SecondaryColor}";
            string model = $"{vehName}";

            UiTools.Dispatch("Parking Violation", $"~b~Veh~s~: {model}~n~~b~Color~s~: {color}~n~~b~Plate~s~: {parkedVehicle.Fx.Mods.LicensePlate}");

            base.IsSetup = true;
        }
        internal override void End(bool forcefully = false, CalloutMessage cm = null)
        {
            if (!parkedVehicle.Exists())
            {
                calloutMessage.IsCalloutFinished = true;
            }

            base.End(forcefully, cm);

            if (blip != null)
            {
                if (blip.Exists())
                {
                    blip.Delete();
                }
            }
        }

        internal override void Tick()
        {
#if DEBUG
            if (PlayerManager.IsDeveloper && Game.PlayerPed.Position.Distance(parkedVehicle.Position) > 50f)
                Screen.ShowSubtitle($"Dis: {Game.PlayerPed.Position.Distance(parkedVehicle.Position)}");
#endif

            switch (progress)
            {
                case 1:
                    if (Game.PlayerPed.Position.Distance(parkedVehicle.Position) > 50f) return;
                    
                    Screen.ShowNotification($"Process the vehicle and tow it away");

                    blip.Delete();

                    blip = parkedVehicle.Fx.AttachBlip();
                    blip.Color = BlipColor.MichaelBlue;

                    blip.Sprite = (BlipSprite)143;
                    blip.Scale = .8f;

                    API.ShowHeightOnBlip(blip.Handle, true);

                    progress++;
                    break;
            }
        }

        internal static string GetVehDisplayNameFromModel(int hash) => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)hash));
    }
}
