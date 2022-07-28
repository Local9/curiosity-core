using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Models;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    public class PropertyManager : Manager<ParticleManager>
    {
        List<Building> _buildings = new();
        const string PROP_SALE_SIGN = "prop_forsale_dyn_01";

        public override async void Begin()
        {
            await Session.Loading();

            Building _ThreeAltaStreet = new();
            _ThreeAltaStreet.Name = "3 Alta St";
            _ThreeAltaStreet.Enterance = new Quaternion(-259.8061f, -969.4397f, 30.21999f, 70.91596f);
            _ThreeAltaStreet.Exit = new Quaternion(-261.1243f, -972.8566f, 30.21996f, 203.0815f);
            _ThreeAltaStreet.Lobby = new Quaternion(-263.679f, -966.7826f, 30.22428f, 204.0962f);
            _ThreeAltaStreet.Camera = new BuildingCamera(new Vector3(-215.2378f, -1071.639f, 32.85828f), new Vector3(22.62831f, 0f, 26.93762f), 50.0f);
            _ThreeAltaStreet.EnteranceCamera1 = new BuildingCamera(new Vector3(-261.2893f, -985.9326f, 34.31419f), new Vector3(-15.94485f, 0, 0.3253375f), 50.0f);
            _ThreeAltaStreet.EnteranceCamera2 = new BuildingCamera(new Vector3(-261.2893f, -985.9326f, 34.31419f), new Vector3(65.94485f, 0, 0.3253375f), 50.0f);
            _ThreeAltaStreet.EnteranceCamera3 = new BuildingCamera(new Vector3(-288.8877f, -994.0138f, 24.12381f), new Vector3(-3.488037f, -0.0000001069198f, -100.3268f), 50.0f); // Garage Camera
            _ThreeAltaStreet.EnteranceCamera4 = new BuildingCamera(new Vector3(-298.3831f, -990.7642f, 24.12381f), new Vector3(-3.879195f, -0.00000001337085f, -87.55238f), 50.0f); // Garage Camera
            _ThreeAltaStreet.BuildingType = eBuildingType.Apartment;
            _ThreeAltaStreet.ExteriorIndex = 5;
            _ThreeAltaStreet.FrontDoor = eFrontDoor.DoubleDoors;
            _ThreeAltaStreet.Door1 = new Door("hei_prop_dt1_20_mph_door_l", new Vector3(-263.461f, -970.5215f, 31.60709f)); // Front Door Left
            _ThreeAltaStreet.Door2 = new Door("hei_prop_dt1_20_mph_door_r", new Vector3(-260.6575f, -969.2133f, 31.60706f)); // Front Door Right
            _ThreeAltaStreet.Door3 = new Door("hei_prop_dt1_20_mp_gar2", new Vector3(-282.5465f, -995.163f, 24.68051f)); // Garage Door
            _ThreeAltaStreet.SaleSign = new SaleSign(PROP_SALE_SIGN, new Quaternion(-252.6184f, -970.720764f, 30.22f, -20.0f));
            _ThreeAltaStreet.Garage = new();
            _ThreeAltaStreet.Garage.CarEnterance = new Quaternion(-279.7589f, -995.9545f, 24.5305f, 74.48383f);
            _ThreeAltaStreet.Garage.CarExit = new Quaternion(-271.5633f, -999.2233f, 26.0224f, 249.66f);
            _ThreeAltaStreet.Garage.FootEnterance = new Quaternion(-279.7421f, -992.0921f, 23.30595f, 74.48383f);
            _ThreeAltaStreet.Garage.FootExit = new Quaternion(-286.7632f, -993.5939f, 23.13706f, 239.0284f);
            _ThreeAltaStreet.Garage.GarageType = eGarageType.Ten;
            _ThreeAltaStreet.Garage.Door = eFrontDoor.StandardDoor;
            _ThreeAltaStreet.Garage.Waypoint = new Quaternion(-292.5203f, -991.2855f, 23.47978f, 250.2265f);
            _buildings.Add(_ThreeAltaStreet); // Move all of this into a JSON file

            foreach(Building building in _buildings)
            {
                building.CreateBuilding();
            }
        }

        int currentLocation = 5;

        // [TickHandler]
        private async Task OnPropertyManager()
        {
            if (Game.IsControlJustPressed(0, Control.FrontendDown))
                --currentLocation;

            if (Game.IsControlJustPressed(0, Control.FrontendUp))
                ++currentLocation;

            if (currentLocation < 0)
                currentLocation = 114;

            if (currentLocation > 114)
                currentLocation = 0;

            DisableExterior(currentLocation);

            int currentInterior = GetInteriorFromEntity(Game.PlayerPed.Handle);

            // Screen.DisplayHelpTextThisFrame($"Current Location: {currentLocation}~n~Int: {currentInterior}");
        }

        private void DisableExterior(int location)
        {
            uint hashKey;
            // GetHashKey("mpsv_lp0_31"); == 79

            SetDisableDecalRenderingThisFrame();

            switch (location)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 61:
                case 83:
                case 84:
                case 85:
                    hashKey = (uint)GetHashKey("apa_ss1_11_flats");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ss1_11_ss1_emissive_a");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ss1_11_detail01b");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ss1_11_Flats_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_02_Building01_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_LOD_01_02_08_09_10_11");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_02_SLOD1");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 5:
                case 6:
                    hashKey = (uint)GetHashKey("hei_dt1_20_build2");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("dt1_20_dt1_emissive_dt1_20");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 7:
                case 34:
                case 62:
                    hashKey = (uint)GetHashKey("sm_14_emissive");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("hei_sm_14_bld2");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 35:
                case 36:
                case 37:
                    hashKey = (uint)GetHashKey("hei_bh1_09_bld_01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("bh1_09_ema");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableExteriorCullModelThisFrame((uint)GetHashKey("prop_wall_light_12a"));
                    DisableOcclusionThisFrame();
                    break;

                case 38:
                case 39:
                case 65:
                    hashKey = (uint)GetHashKey("hei_dt1_03_build1x");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("DT1_Emissive_DT1_03_b1");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("dt1_03_dt1_Emissive_b1");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 40:
                case 41:
                case 63:
                    hashKey = (uint)GetHashKey("hei_bh1_08_bld2");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("bh1_emissive_bh1_08");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("bh1_08_bld2_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("hei_bh1_08_bld2");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("bh1_08_em");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 42:
                case 43:
                    hashKey = (uint)GetHashKey("apa_ss1_02_building01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_Emissive_SS1_02a_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ss1_02_ss1_emissive_ss1_02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 64:
                    hashKey = (uint)GetHashKey("hei_ss1_02_building01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_Emissive_SS1_02a_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ss1_02_ss1_emissive_ss1_02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ss1_02_building01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("SS1_02_Building01_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 73:
                    hashKey = (uint)GetHashKey("apa_ch2_05e_res5");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_05e_res5_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 74:
                    hashKey = (uint)GetHashKey("apa_ch2_04_house02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_04_house02_d");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_04_M_a_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_house02_railings");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_emissive_04");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_emissive_04_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_04_house02_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 75:
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs01a_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs01_balcony");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_Emissive_11_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_Emissive_11");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_CH2_09b_House08_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 76:
                    hashKey = (uint)GetHashKey("apa_ch2_09c_hs11");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("CH2_09c_Emissive_11_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("CH2_09c_Emissive_11");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09c_hs11_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 77:
                    hashKey = (uint)GetHashKey("apa_ch2_05c_b4");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_05c_emissive_07");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_05c_decals_05");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_05c_B4_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 78:
                    hashKey = (uint)GetHashKey("apa_ch2_09c_hs07");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_09c_build_11_07_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("CH2_09c_Emissive_07_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09c_build_11_07_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_09c_hs07_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("CH2_09c_Emissive_07");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 79:
                    hashKey = (uint)GetHashKey("apa_ch2_09c_hs13");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09c_hs13_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_CH2_09c_House11_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_09c_Emissive_13_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_09c_Emissive_13");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 80:
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs02b_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_Emissive_09_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_09b_botpoolHouse02_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_Emissive_09");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_09b_hs02_balcony");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 81:
                    hashKey = (uint)GetHashKey("apa_ch2_12b_house03mc");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_12b_emissive_02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_12b_house03_MC_a_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_12b_emissive_02_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_12b_railing_06");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 82:
                    hashKey = (uint)GetHashKey("apa_ch2_04_house01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_04_house01_d");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_emissive_05_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("apa_ch2_04_M_b_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_emissive_05");
                    EnableExteriorCullModelThisFrame(hashKey);
                    hashKey = (uint)GetHashKey("ch2_04_house01_details");
                    EnableExteriorCullModelThisFrame(hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 87:
                case 103:
                case 104:
                case 105:
                    hashKey = (uint)GetHashKey("sm_13_emissive");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_13_bld1");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_13_bld1_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 88:
                case 106:
                case 107:
                case 108:
                    hashKey = (uint)GetHashKey("sm_15_bld2_dtl");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("hei_sm_15_bld2");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_bld2_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_bld2_dtl3");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_bld1_dtl3");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_bld2_railing");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_emissive");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("sm_15_emissive_LOD");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 89:
                case 109:
                case 110:
                case 111:
                    hashKey = (uint)GetHashKey("hei_dt1_02_w01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("dt1_02_helipad_01");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("dt1_02_dt1_emissive_dt1_02");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    DisableOcclusionThisFrame();
                    break;

                case 90:
                case 112:
                case 113:
                case 114:
                    hashKey = (uint)GetHashKey("dt1_11_dt1_emissive_dt1_11");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    hashKey = (uint)GetHashKey("dt1_11_dt1_tower");
                    EnableExteriorCullModelThisFrame(hashKey);
                    EnableScriptCullModelThisFrame((int)hashKey);
                    DisableOcclusionThisFrame();
                    break;
            }
        }
    }
}
