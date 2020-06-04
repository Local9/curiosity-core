using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Menu
{
    class PanicButton
    {
        private static PluginManager PluginInstance => PluginManager.Instance;
        private static bool IsPanicButtonCooldownActive = false;

        private static List<Ped> cops = new List<Ped>();

        public async static void Pressed()
        {
            if (IsPanicButtonCooldownActive)
            {
                UiTools.Dispatch("Request Denied", "Requested units are currently responding to another call");
                return;
            }

            IsPanicButtonCooldownActive = true;
            PluginInstance.RegisterTickHandler(OnPanicButtonCooldown);

            int numCops = Utility.RANDOM.Next(3, 10);

            for (var i = 0; i < numCops; i++)
            {
                PedHash pedToSpawn;
                VehicleHash vehicleHash;

                switch (PlayerManager.PatrolZone)
                {
                    case PatrolZone.Highway:
                        pedToSpawn = Collections.PolicePeds.HIGHWAY.Random();
                        vehicleHash = Collections.PoliceCars.HIGHWAY.Random();
                        break;
                    case PatrolZone.Country:
                    case PatrolZone.Rural:
                        pedToSpawn = Collections.PolicePeds.RURAL.Random();
                        vehicleHash = Collections.PoliceCars.RURAL.Random();
                        break;
                    default:
                        pedToSpawn = Collections.PolicePeds.URBAN.Random();
                        vehicleHash = Collections.PoliceCars.URBAN.Random();
                        break;
                }


                Vehicle copCar = await World.CreateVehicle(vehicleHash,
                    Game.PlayerPed.Position.AroundStreet(200f, 2000f));
                copCar.IsSirenActive = true;

                Ped cop = await World.CreatePed(pedToSpawn, copCar.Position + copCar.UpVector * 5f);
                cop.SetIntoVehicle(copCar, VehicleSeat.Driver);
                cop.Weapons.Give(WeaponHash.APPistol, 90, false, true);
                Blip blip = cop.AttachedBlip;

                if (blip == null)
                {
                    blip = cop.AttachBlip();
                }

                if (blip != null)
                {
                    blip.Color = BlipColor.Blue;
                    blip.IsFriendly = true;
                }

                TaskSequence sequence = new TaskSequence();

                sequence.AddTask.DriveTo(copCar, Game.PlayerPed.Position, 15f, float.MaxValue, (int)DrivingStyle.Rushed);
                sequence.AddTask.LeaveVehicle();
                sequence.AddTask.ChatTo(Game.PlayerPed);
                sequence.AddTask.WanderAround();
                sequence.Close();

                cop.Task.PerformSequence(sequence);

                copCar.IsPersistent = false;

                cops.Add(cop);
            }
        }

        static async Task OnPanicButtonCooldown()
        {
            long ggt = API.GetGameTimer();
            while ((API.GetGameTimer() - ggt) < 300000) // 5 minutes
            {
                if ((API.GetGameTimer() - ggt) > 120000)
                {
                    if (cops.Count > 0)
                    {
                        cops.ForEach(c =>
                        {
                            if (c != null)
                            {
                                if (c.Exists())
                                    c.IsPersistent = false;
                            }
                        });
                    }
                }

                await BaseScript.Delay(100);
            }
            cops.Clear();
            UiTools.Dispatch("Units Available", "Units have now returned to the depo");
            IsPanicButtonCooldownActive = false;
            PluginInstance.DeregisterTickHandler(OnPanicButtonCooldown);
        }
    }
}
