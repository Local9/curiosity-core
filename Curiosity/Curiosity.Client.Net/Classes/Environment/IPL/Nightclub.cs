using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment.IPL
{
    class Nightclub
    {
        static Client client = Client.GetInstance();

        static int interiorId = 271617;
        static string interiorIpl = "ba_int_placement_ba_interior_0_dlc_int_01_ba_milo_";
        static bool IsPlayerInNightClub = false;
        static string currentDj = "solomon";
        static string banner = $"ba_case7_{currentDj}";
        static string barrier = "ba_barriers_case7";
        static Vector3 clubLocation = new Vector3(-1569.226f, -3017.124f, -74.40616f);
        static Vector3 clubEntrance = new Vector3(194.6124f, -3167.278f, 5.790269f);

        static List<string> interiorProps = new List<string>()
        {
            "Int01_ba_security_upgrade",
            //"Int01_ba_equipment_setup",
            "Int01_ba_Style02",
            "DJ_01_Lights_02",
            //"Int01_ba_style01_podium",
            "Int01_ba_style02_podium",
            "int01_ba_lights_screen",
            "Int01_ba_Screen",
            "Int01_ba_bar_content",
            "Int01_ba_booze_01",
            "Int01_ba_booze_02",
            "Int01_ba_booze_03",
            "Int01_ba_dj01",
            "Int01_ba_lightgrid_01",
            "Int01_ba_Clutter",
            "Int01_ba_clubname_02",
            "Int01_ba_dry_ice",
            "Int01_ba_deliverytruck"
        };

        public static void Init()
        {
            if (!IsIplActive(banner))
            {
                RequestIpl(banner);
            }

            if (!IsIplActive(barrier))
            {
                RequestIpl(barrier);
            }

            new BlipData(clubEntrance, (BlipSprite)614, Shared.Client.net.Enums.BlipCategory.Unknown, BlipColor.Blue, true).Create();

            client.RegisterTickHandler(TeleportToClub);
        }

        static async Task TeleportToClub()
        {
            if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubEntrance, true) < 2.0
                && ControlHelper.IsControlPressed(Control.Context, false)
                && !IsPlayerInNightClub)
            {
                DoScreenFadeOut(500);
                await BaseScript.Delay(350);
                LoadNightClubInterior();
            }

            if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubEntrance, true) < 2.0)
            {
                NativeWrappers.DrawHelpText("Press ~INPUT_PICKUP~ to ~b~enter the club");
            }

            await Task.FromResult(0);
        }

        // new WarpPointPair { A = new WarpPoint("go outside", new Vector3(195.0901f, -3167.226f, 5.790268f + MarkerZAdjustment), 100.0f), 
        // B = new WarpPoint("go inside", new Vector3(-1569.226f, -3017.124f, -74.40616f + MarkerZAdjustment), 0.0f), },

        static async void LoadNightClubInterior()
        {
            while (true)
            {
                if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubEntrance, true) < 2.0 && !IsPlayerInNightClub)
                {
                    interiorId = GetInteriorAtCoordsWithType(-1604.664f, -3012.583f, -79.9999f, "ba_dlc_int_01_ba");
                    if (IsValidInterior(interiorId))
                    {

                        LoadInterior(interiorId);

                        if (IsInteriorReady(interiorId))
                        {
                            await PrepareNightclubInterior();

                            Game.PlayerPed.Position = new Vector3(-1569.226f, -3017.124f, -74.40616f);
                            Game.PlayerPed.Heading = 15.0804f;
                            GameplayCamera.RelativeHeading = 12.1322f;
                            GameplayCamera.RelativePitch = -3.2652f;
                            DoScreenFadeIn(500);
                            await BaseScript.Delay(350);
                            StartAudioScene("DLC_Ba_NightClub_Scene");
                            PlaySoundFrontend(-1, "club_crowd_transition", "dlc_btl_club_open_transition_crowd_sounds", true);
                        }
                    }
                }
                await BaseScript.Delay(5);
            }
        }

        static async Task PrepareNightclubInterior()
        {
            foreach(string interiorProp in interiorProps)
            {
                if (IsInteriorPropEnabled(interiorId, interiorProp))
                {
                    EnableInteriorProp(interiorId, interiorProp);
                    await BaseScript.Delay(30);
                }
            }

            if (!IsIplActive(interiorIpl))
                RequestIpl(interiorIpl);

            RequestScriptAudioBank("DLC_BATTLE/BTL_CLUB_OPEN_TRANSITION_CROWD", false);
            SetAmbientZoneStatePersistent("IZ_ba_dlc_int_01_ba_Int_01_main_area", false, false);
            await BaseScript.Delay(100);
            RefreshInterior(interiorId);

            IsPlayerInNightClub = true;

            await Task.FromResult(0);
        }
    }
}
