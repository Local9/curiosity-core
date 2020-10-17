using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.NaturalMotion;
using Curiosity.CasinoSystems.Client.Extensions;
using Curiosity.Global.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts.InteriorScripts
{
    class Population
    {
        static Plugin plugin;

        // Ped
        static Ped ped;

        public static async void Init()
        {
            plugin = Plugin.GetInstance();

            RemovePed();

            ped = await Creators.CreatePed(PedTypes.PED_TYPE_CIVFEMALE, API.GetHashKey("MP_M_ExecPA_01"), new Vector3(1093.526f, 211.2489f, -48.99986f), 80.98913f);

            if (ped.Exists())
            {
                API.SetEntityCanBeDamaged(ped.Handle, false);
                API.SetEntitySomething(ped.Handle, true);
                API.SetBlockingOfNonTemporaryEvents(ped.Handle, true);
                API.SetPedAsEnemy(ped.Handle, false);
                API.SetPedDefaultComponentVariation(ped.Handle);
                ped.SetConfigFlag(185, true);
                ped.SetConfigFlag(208, true);
                ped.SetConfigFlag(108, true);
                API.SetPedCanEvasiveDive(ped.Handle, false);
                API.N_0x2f3c3d9f50681de4(ped.Handle, true);
                API.SetPedCanRagdollFromPlayerImpact(ped.Handle, false);
                API.SetPedCanBeTargetted(ped.Handle, false);

                API.SetPedComponentVariation(ped.Handle, 0, 0, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 2, 0, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 6, 0, 1, 0);
                API.SetPedComponentVariation(ped.Handle, 7, 3, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 8, 0, 0, 0);
                API.SetPedComponentVariation(ped.Handle, 11, 0, 0, 0);
            }
        }

        public static void Dispose()
        {
        }

        static void RemovePed()
        {
            if (ped != null)
            {
                if (ped.Exists())
                    ped.Delete();
            }
        }
    }
}
