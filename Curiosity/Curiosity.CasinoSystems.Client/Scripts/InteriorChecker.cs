using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.CasinoSystems.Client.Scripts.InteriorScripts;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.CasinoSystems.Client.Scripts
{
    class InteriorChecker
    {
        static Plugin plugin;
        static bool isPlayerInsideCasino;
        static bool playerPreviousState;
        static int interiorId = 275201;

        static string currentWeather;

        static bool hasRequestedAudio = false;

        public static void Init()
        {
            plugin = Plugin.GetInstance();

            plugin.RegisterTickHandler(OnCasinoInteriorCheck);

            plugin.RegisterEventHandler("curiosity:Client:Weather:Sync", new Action<string, bool, float, float, bool, bool>(WeatherSync));

            RequestIpl("vw_casino_main"); // move
            RequestIpl("vw_casino_carpark");
            RequestIpl("vw_casino_garage");
            RequestIpl("vw_casino_penthouse");

            RequestIpl("hei_dlc_casino_aircon");
            RequestIpl("hei_dlc_casino_aircon_lod");
            RequestIpl("hei_dlc_casino_door");
            RequestIpl("hei_dlc_casino_door_lod");
            RequestIpl("hei_dlc_vw_roofdoors_locked");
            RequestIpl("hei_dlc_windows_casino");
            RequestIpl("hei_dlc_windows_casino_lod");

            RequestIpl("vw_ch3_additions");
            RequestIpl("vw_ch3_additions_long_0");
            RequestIpl("vw_ch3_additions_strm_0");

            RequestIpl("vw_dlc_casino_door");
            RequestIpl("vw_dlc_casino_door_lod");
            RequestIpl("vw_casino_billboard");
            RequestIpl("vw_casino_billboard_lod(1)");
            RequestIpl("vw_casino_billboard_lod");
            RequestIpl("vw_int_placement_vw");
            RequestIpl("vw_dlc_casino_apart");
        }

        private static async Task OnCasinoInteriorCheck()
        {
            int playerInterior = API.GetInteriorFromEntity(Game.PlayerPed.Handle);
            isPlayerInsideCasino = playerInterior == interiorId;

            if (isPlayerInsideCasino)
            {
                Game.DisableControlThisFrame(0, Control.Jump);
                Game.Player.DisableFiringThisFrame();
                API.HudForceWeaponWheel(false);
                Game.PlayerPed.CanSwitchWeapons = false;
                Game.PlayerPed.Weapons.Give(WeaponHash.Unarmed, 0, true, true);
            }

            if (playerPreviousState == isPlayerInsideCasino) return;

            playerPreviousState = isPlayerInsideCasino;

            if (isPlayerInsideCasino)
            {
                BaseScript.TriggerEvent("curiosity:Player:World:FreezeTimer", true);
                plugin.RegisterTickHandler(OnWeatherAndClockLock);

                AudioSettings();

                // NOTE: Look into awaiting each interior script before allowing player to see.

                VehiclePodium.Init();
                // CasinoBar.Init();
                // Population.Init();
                // LuckyWheel.Init();
            }
            else
            {
                API.SetRadarZoomPrecise(0f);

                hasRequestedAudio = false;

                // unknown, need to test.
                Game.PlayerPed.SetConfigFlag(342, false);
                Game.PlayerPed.SetConfigFlag(429, false);
                API.N_0xa9b61a329bfdcbea(Game.PlayerPed.Handle, true);

                if (!API.NetworkIsActivitySession())
                {
                    Game.PlayerPed.SetConfigFlag(122, false);
                    Game.PlayerPed.IsInvincible = false;
                    API.SetEntityCanBeDamaged(Game.PlayerPed.Handle, true);
                    API.SetPedCanRagdollFromPlayerImpact(Game.PlayerPed.Handle, true);
                    Game.PlayerPed.SetConfigFlag(106, false);
                }

                if (API.IsStreamPlaying())
                {
                    API.StopStream();
                }

                if (API.IsAudioSceneActive("DLC_VW_Casino_General"))
                {
                    API.StopAudioScene("DLC_VW_Casino_General");
                }

                Game.PlayerPed.CanSwitchWeapons = true;
                BaseScript.TriggerEvent("curiosity:Player:World:FreezeTimer", false);
                plugin.DeregisterTickHandler(OnWeatherAndClockLock);
                API.SetOverrideWeather(currentWeather);

                VehiclePodium.Dispose();
                CasinoBar.Dispose();
                // Population.Dispose();
                LuckyWheel.Dispose();
            }
            await BaseScript.Delay(500);
        }

        static void AudioSettings()
        {
            if (!hasRequestedAudio)
            {
                if((((API.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false)
                    && API.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false))
                    && API.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false))
                    && API.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false))
                    && API.LoadStream("casino_walla", "DLC_VW_Casino_Interior_Sounds"))
                {
                    hasRequestedAudio = true;
                }
            }

            if (!API.IsAudioSceneActive("DLC_VW_Casino_General"))
            {
                API.StartAudioScene("DLC_VW_Casino_General");
            }
        }

        private static async Task OnWeatherAndClockLock()
        {
            API.NetworkOverrideClockTime(18, 1, 0);
            API.SetOverrideWeather("CLEAR");
        }

        private static void WeatherSync(string weather, bool arg2, float arg3, float arg4, bool arg5, bool arg6)
        {
            currentWeather = weather;
        }
    }
}
