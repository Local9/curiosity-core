using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.CasinoSystems.Client.Extensions;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts.InteriorScripts
{
    class CasinoBar
    {
        static Plugin plugin;

        // Ped
        static Ped BarPed;

        public static async void Init()
        {
            plugin = Plugin.GetInstance();

            RemovePed();

            BarPed = await Creators.CreatePed(PedTypes.PED_TYPE_CIVFEMALE, API.GetHashKey("s_f_y_clubbar_01"), new Vector3(1110.362f, 208.4152f, -49.44012f), 80.98913f);
            BarPed.IsPositionFrozen = true;
        }

        public static void Dispose()
        {
        }

        static void RemovePed()
        {
            if (BarPed != null)
            {
                if (BarPed.Exists())
                    BarPed.Delete();
            }
        }
    }
}
