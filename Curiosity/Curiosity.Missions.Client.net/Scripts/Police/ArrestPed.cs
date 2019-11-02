using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Scripts.PedCreators;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;
using Curiosity.Missions.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class ArrestPed
    {
        static Client client = Client.GetInstance();

        static Dictionary<int, Ped> arrestedPeds = new Dictionary<int, Ped>();

        public static void Init()
        {
            // kill it incase it doubles
            //client.DeregisterTickHandler(OnTask);

            arrestedPeds.Clear();

            //client.RegisterTickHandler(OnTask);
        }

        public static void Dispose()
        {
            arrestedPeds.Clear();

            //client.DeregisterTickHandler(OnTask);
        }

        static async Task OnTask()
        {
            await Task.FromResult(0);

            if (Game.PlayerPed.IsAiming)
            {
                int entityIdPlayerIsAimingAt = 0;
                API.GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entityIdPlayerIsAimingAt);

                if (entityIdPlayerIsAimingAt == 0) return;
                
                if (API.IsEntityAPed(entityIdPlayerIsAimingAt))
                {
                    if (arrestedPeds.ContainsKey(entityIdPlayerIsAimingAt))
                    {
                        Ped ped = arrestedPeds[entityIdPlayerIsAimingAt];

                        ShouldPutHandsUp(ped);

                        return; // END HERE
                    }

                    NormalPed foundPed = NormalPedCreator.Setup(new Ped(entityIdPlayerIsAimingAt), canArrest: true);

                    if (foundPed.IsInVehicle) return;

                    arrestedPeds.Add(entityIdPlayerIsAimingAt, foundPed);

                    ShouldPutHandsUp(foundPed);
                }
            }

            await Client.Delay(100);
        }

        static void ShouldPutHandsUp(Ped ped)
        {
            if (ped.Position.Distance(Game.PlayerPed.Position) > 5f)
            {
                ped.Task.FleeFrom(Game.PlayerPed, -1);
            }
            else
            {
                if (!API.GetIsTaskActive(ped.Handle, 0) && !ped.IsHandsUp())
                {
                    ped.Task.ClearAllImmediately();
                    ped.Task.HandsUp(-1);
                }
            }
        }
    }
}
