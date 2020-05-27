using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.CasinoSystems.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts.InteriorScripts
{
    class LuckyWheel
    {
        static Plugin plugin;

        static int wheelObjectId;

        static Vector3 wheelPos = new Vector3(1111.052f, 229.8579f, -49.133f);

        public static void Init()
        {
            plugin = Plugin.GetInstance();

            plugin.RegisterTickHandler(OnLuckyWheelMessages);

            CreateProp();
            RequestGraphics();
        }

        private static async Task OnLuckyWheelMessages()
        {
            // Helpers.DisplayHelp("CASINO_LUCK_WD", -1); // Try again later
            // Helpers.DisplayHelpWithNumber("CAS_MG_MEMB2", 20000, -1); // Try again later
            // Helpers.DisplayHelp("LUCKY_WHEEL_US", -1); // One per day
            // Helpers.DisplayHelp("LW_PLAY", -1); // Press E to Spin
            // Helpers.DisplayHelp("POD_TOO_MANY", -1); // Too many players near activity
            // Helpers.DisplayHelp("CAS_LW_REGL", -1); // Feature is not available for you
            // Helpers.IsTextHelpBeingDisplayed("LW_PLAY"); // One per day

            if (API.IsEntityInAngledArea(Game.PlayerPed.Handle, 1110.995f, 228.9034f, -50.6408f, 1109.727f, 228.9352f, -48.3908f, 1.5f, false, true, 0))
            {
                Helpers.DisplayHelp("LW_PLAY", -1); // Press E to Spin
            }
        }

        private static async void CreateProp()
        {
            if (!API.DoesEntityExist(wheelObjectId))
            {
                Model model = LuckyWheelHash();
                model.Request(1000);

                int loadCheck = 0;

                while(!model.IsLoaded)
                {
                    if (loadCheck > 10)
                        break;

                    await BaseScript.Delay(10);
                    loadCheck++;
                }

                if (model.IsLoaded)
                {
                    wheelObjectId = API.CreateObjectNoOffset((uint)LuckyWheelHash(), wheelPos.X, wheelPos.Y, wheelPos.Z, false, false, true);
                    API.SetEntitySomething(wheelObjectId, true);
                    API.SetEntityCanBeDamaged(wheelObjectId, false);

                    model.MarkAsNoLongerNeeded();
                }
            }
        }

        public static void Dispose()
        {
            if (API.DoesEntityExist(wheelObjectId))
            {
                API.DeleteEntity(ref wheelObjectId);
            }

            plugin.DeregisterTickHandler(OnLuckyWheelMessages);
        }

        static void RequestGraphics()
        {
            API.RequestStreamedTextureDict("CasinoUI_Lucky_Wheel", false);
        }

        static int LuckyWheelHash()
        {
            return API.GetHashKey("vw_prop_vw_luckywheel_02a");
        }
    }
}
