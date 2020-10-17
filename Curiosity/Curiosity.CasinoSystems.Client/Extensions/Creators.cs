using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Extensions
{
    class Creators
    {
        public static async Task<Ped> CreatePed(PedTypes pedType, Model model, Vector3 position, float heading)
        {
            int loadChecks = 0;
            model.Request(10000);

            while (!model.IsLoaded)
            {
                if (loadChecks > 10)
                    break;

                loadChecks++;
                await BaseScript.Delay(10);
            }

            if (model.IsLoaded)
            {

                float groundZ = position.Z;
                Vector3 groundNormal = Vector3.Zero;

                if (API.GetGroundZAndNormalFor_3dCoord(position.X, position.Y, position.Z, ref groundZ, ref groundNormal))
                {
                    position.Z = groundZ;
                }

                int pedHandle = API.CreatePed((int)pedType, (uint)model.Hash, position.X, position.Y, position.Z, heading, false, true);
                API.SetPedDefaultComponentVariation(pedHandle);

                Ped ped = new Ped(pedHandle);
                return ped;
            }
            return null;
        }
    }
}
