using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment
{
    class PedHandler
    {
        static Client client = Client.GetInstance();

        static List<CitizenFX.Core.Ped> SpawnedPeds = new List<CitizenFX.Core.Ped>();
        static List<CitizenFX.Core.Blip> SpawnedPedBlips = new List<CitizenFX.Core.Blip>();

        public static void Init()
        {
            ManageBlips();
        }

        public static void AddPed(CitizenFX.Core.Ped ped)
        {
            SpawnedPeds.Add(ped);
        }

        public static void RemovePed(CitizenFX.Core.Ped ped)
        {
            SpawnedPeds.Remove(ped);
        }

        public static async void ManageBlips()
        {
            while (true)
            {
                await Client.Delay(1);
                List<CitizenFX.Core.Ped> SpawnedPedsCopy = SpawnedPeds;
                foreach (CitizenFX.Core.Ped ped in SpawnedPedsCopy)
                {
                    if (ped.IsDead)
                    {
                        ped.AttachedBlip.Delete();
                        RemovePed(ped);
                    }
                }
            }
        }
    }
}
