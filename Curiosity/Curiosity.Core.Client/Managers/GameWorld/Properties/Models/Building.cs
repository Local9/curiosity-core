using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Building
    {
        private const BlipColor BLIP_COLOR_BLACK = (BlipColor)40;
        private const BlipColor BLIP_COLOR_WHITE = (BlipColor)4;
        private bool _hideHud;

        public string Name { get; set; }
        public Quaternion Enterance { get; set; }
        public Quaternion Exit { get; set; }
        public Quaternion Lobby { get; set; }
        public BuildingCamera Camera { get; set; }
        public BuildingCamera EnteranceCamera1 { get; set; }
        public BuildingCamera EnteranceCamera2 { get; set; }
        public BuildingCamera GarageCamera1 { get; set; }
        public BuildingCamera GarageCamera2 { get; set; }
        public eBuildingType BuildingType { get; set; }
        public List<Apartment> Apartments { get; set; } = new();
        public int ExteriorIndex { get; set; }
        public SaleSign SaleSign { get; set; }
        public eFrontDoor FrontDoor { get; set; }
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }
        public Door Door3 { get; set; }
        public Blip BuildingBlip { get; set; }
        public Blip SaleSignBlip { get; set; }
        public Garage Garage { get; set; }

        public bool BuildingSetup { get; set; }

        public void CreateBuilding()
        {
            SetupBlip();
            SaleSign.CreateForSaleSign();
        }

        void SetupBlip()
        {
            BuildingBlip = World.CreateBlip(Enterance.AsVector());
            BuildingBlip.IsShortRange = true;
            SetBlipCategory(BuildingBlip.Handle, 10); // 10 - Property / 11 = Owned Property

            // Need to know what ones the player owns?
            // Local KVP Store?

            switch (BuildingType)
            {
                case eBuildingType.Apartment:
                    BuildingBlip.Sprite = BlipSprite.SafehouseForSale;
                    BuildingBlip.Name = Game.GetGXTEntry("MP_PROP_SALE1");
                    break;
            }

            // BuildingBlip.Color = BLIP_COLOR_BLACK;
        }

        public void ToggleDoors(bool unlock = false)
        {
            if (unlock)
            {
                switch (FrontDoor)
                {
                    case eFrontDoor.DoubleDoors:
                        Door1.Unlock();
                        Door2.Unlock();
                        break;
                    case eFrontDoor.StandardDoor:
                        Door1.Unlock();
                        break;
                }
                return;
            }
            switch (FrontDoor)
            {
                case eFrontDoor.DoubleDoors:
                    Door1.Lock();
                    Door2.Lock();
                    break;
                case eFrontDoor.StandardDoor:
                    Door1.Lock();
                    break;
            }
        }

        public async Task PlayEnterApartmentCamera(int duration, bool easePosition, bool easeRotation, CameraShake cameraShake, float cameraShakeAmplitude)
        {
            Cache.Player.DisableHud();
            ToggleDoors(true);
            Game.PlayerPed.Task.GoTo(Lobby.AsVector(), true, 7000);
            Camera scriptCamera = World.CreateCamera(EnteranceCamera1.Position, EnteranceCamera1.Rotation, EnteranceCamera1.FieldOfView);
            Camera interpCamera = World.CreateCamera(EnteranceCamera2.Position, EnteranceCamera2.Rotation, EnteranceCamera2.FieldOfView);
            World.RenderingCamera = scriptCamera;
            scriptCamera.InterpTo(interpCamera, duration, easePosition, easeRotation);
            World.RenderingCamera = interpCamera;
            interpCamera.Shake(cameraShake, cameraShakeAmplitude);
            await BaseScript.Delay(duration);
            ToggleDoors(false);
            Cache.Player.EnableHud();
        }

        public async Task PlayExitApartmentCamera(int duration, bool easePosition, bool easeRotation, CameraShake cameraShake, float cameraShakeAmplitude)
        {
            Cache.Player.DisableHud();
            Game.PlayerPed.Position = Lobby.AsVector();
            Game.PlayerPed.Heading = Exit.W;
            ToggleDoors(true);
            Game.PlayerPed.Task.GoTo(Exit.AsVector(), true, 7000);
            Camera scriptCamera = World.CreateCamera(EnteranceCamera2.Position, EnteranceCamera2.Rotation, EnteranceCamera2.FieldOfView);
            Camera interpCamera = World.CreateCamera(EnteranceCamera1.Position, EnteranceCamera1.Rotation, EnteranceCamera1.FieldOfView);
            World.RenderingCamera = scriptCamera;
            scriptCamera.InterpTo(interpCamera, duration, easePosition, easeRotation);
            World.RenderingCamera = interpCamera;
            interpCamera.Shake(cameraShake, cameraShakeAmplitude);
            await BaseScript.Delay(duration);
            World.DestroyAllCameras();
            World.RenderingCamera = null;
            ToggleDoors(false);
            Cache.Player.EnableHud();
        }



        private void DisableExterior()
        {
            uint hashKey;
            // GetHashKey("mpsv_lp0_31"); == 79

            SetDisableDecalRenderingThisFrame();

            switch (ExteriorIndex)
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
