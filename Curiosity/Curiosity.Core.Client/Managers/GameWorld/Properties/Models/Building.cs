using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;
using NativeUI;
using System.Drawing;
using System.Linq;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Building
    {
        private Prop _propForSaleSign;

        public UIMenu MenuBuyApartment;
        public UIMenu MenuApartment;
        public UIMenu MenuGarage;

        UIMenuItem exitMenu = new UIMenuItem("Close Menu");

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
        // Garage
        public eGarageType GarageType { get; set; }
        public Quaternion GarageCarEnterance { get; set; }
        public Quaternion GarageCarExit { get; set; }
        public Quaternion GarageFootEnterance { get; set; }
        public Quaternion GarageFootExit { get; set; }
        public Quaternion GarageElevator { get; set; }
        public Quaternion GarageMenuPosition { get; set; }
        public eFrontDoor GarageDoor { get; set; }
        public Quaternion GarageWaypoint { get; set; }


        BlipData garageBlip = new();
        BlipData buildingBlip = new();

        public bool IsOwnedByPlayer => Apartments.Where(x => x.IsOwnedByPlayer).FirstOrDefault() is not null;

        public bool BuildingSetup { get; set; }

        public async void CreateBuilding()
        {
            SetupBlip();
            CreateForSaleSign();
            await BaseScript.Delay(0);

            CreateBuyMenu();
        }

        void CreateBuyMenu(bool rebuildMenu = false)
        {
            if (!rebuildMenu)
            {
                MenuBuyApartment = new UIMenu("", Game.GetGXTEntry("MP_PROP_GEN0"));
                MenuBuyApartment.SetBannerType(new UIResRectangle(PointF.Empty, new SizeF(0, 0), Color.FromArgb(0, 0, 0, 0)));
                MenuBuyApartment.MouseEdgeEnabled = false;
                MenuBuyApartment.MouseControlsEnabled = false;
                PluginManager.MenuPool.Add(MenuBuyApartment);

                MenuBuyApartment.OnItemSelect += (sender, selectedItem, index) =>
                {
                    if (selectedItem != exitMenu)
                    {
                        NotificationManager.GetModule().Error($"Sorry this feature is currently not enabled");
                        return;
                    }

                    if (selectedItem == exitMenu)
                    {
                        MenuBuyApartment.Visible = false;
                        MenuBuyApartment.RefreshIndex();
                        ResetMenuOnClose();
                    }
                };
            }

            if (!rebuildMenu)
                MenuBuyApartment.Clear();

            foreach (Apartment apartment in Apartments)
            {
                UIMenuItem menuItem = new UIMenuItem(Game.GetGXTEntry(apartment.Name), Game.GetGXTEntry(apartment.Description));
                if (!apartment.IsOwnedByPlayer)
                {
                    menuItem.SetRightLabel($"${apartment.Price:N0}");
                }
                MenuBuyApartment.AddItem(menuItem);
            }

            MenuBuyApartment.AddItem(exitMenu);
        }

        public async void ResetMenuOnClose()
        {
            await ScreenInterface.FadeOut();
            MenuBuyApartment.Visible = false;
            World.DestroyAllCameras();
            World.RenderingCamera = null;
            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.FadeIn(false);
            PluginManager.MenuPool.MouseEdgeEnabled = true;
            PluginManager.MenuPool.CloseAllMenus();
            PluginManager.Instance.DetachTickHandler(PluginManager.OnMenuDisplay);
            MenuBuyApartment.RefreshIndex();
            await ScreenInterface.FadeIn();
            Cache.Player.EnableHud();
        }

        public void OpenBuyMenu()
        {
            if (MenuBuyApartment is null) CreateBuyMenu();

            PluginManager.MenuPool.MouseEdgeEnabled = false;
            MenuBuyApartment.Visible = true;
            PluginManager.Instance.AttachTickHandler(PluginManager.OnMenuDisplay);

            Logger.Info($"Menu State: {MenuBuyApartment.Visible}");
        }

        public bool IsCloseToSaleSign => Game.PlayerPed.IsInRangeOf(SaleSign.Position.AsVector(), 3f);

        public bool IsInRangetOfGarageEnterance(float range)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                return Game.PlayerPed.CurrentVehicle.IsInRangeOf(GarageCarEnterance.AsVector(), range);
            }
            return Game.PlayerPed.IsInRangeOf(GarageFootEnterance.AsVector(), range);
        }

        public bool IsInRangeOfEnterance(float range) => Game.PlayerPed.IsInRangeOf(GarageFootEnterance.AsVector(), range);
        public bool IsInRangeOfGarageElevator(float range) => Game.PlayerPed.IsInRangeOf(GarageElevator.AsVector(), range);
        public bool IsInRangeOfGarageMenu(float range) => Game.PlayerPed.IsInRangeOf(GarageMenuPosition.AsVector(), range);
        public bool IsAtHome => IsInteriorScene();

        public bool IsForSale => (int)BuildingBlip.Sprite == 350 || (int)BuildingBlip.Sprite == 476 || (int)BuildingBlip.Sprite == 369;

        void SetupBlip(bool refresh = false)
        {
            // Blips need to be added to the master handler
            BlipManager blipManager = BlipManager.ManagerInstance;

            if (refresh)
            {
                if (buildingBlip is not null) blipManager.RemoveBlip(buildingBlip.Name);
                if (garageBlip is not null) blipManager.RemoveBlip(garageBlip.Name);
            }

            buildingBlip.Positions.Add(Enterance.ToPosition());
            buildingBlip.IsShortRange = true;
            buildingBlip.Category = IsOwnedByPlayer ? 11 : 10; // 10 - Property / 11 = Owned Property

            // Need to know what ones the player owns?
            // Local KVP Store?

            switch (BuildingType)
            {
                case eBuildingType.Apartment:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 40 : 350;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("CELL_2630") : Game.GetGXTEntry("MP_PROP_SALE1");
                    break;
                case eBuildingType.Office:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 475 : 476;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_475") : Game.GetGXTEntry("MP_PROP_SALE2");
                    break;
                case eBuildingType.Clubhouse:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 374 : 375;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("PM_SPAWN_CLUBH") : Game.GetGXTEntry("BLIP_373");
                    break;
                case eBuildingType.Nightclub:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 614 : 375;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("CELL_CLUB") : Game.GetGXTEntry("BLIP_375");
                    break;
                case eBuildingType.Bunker: // animation
                    buildingBlip.Sprite = IsOwnedByPlayer ? 557 : 375;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_557") : Game.GetGXTEntry("BLIP_375");
                    break;
                case eBuildingType.Garage:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 357 : 369;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_357") : Game.GetGXTEntry("MP_PROP_SALE0");
                    break;
                case eBuildingType.Hanger: // animation?
                    buildingBlip.Sprite = IsOwnedByPlayer ? 359 : 372;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_359") : Game.GetGXTEntry("BLIP_372");
                    break;
                case eBuildingType.Warehouse:
                    buildingBlip.Sprite = IsOwnedByPlayer ? 473 : 474;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_473") : Game.GetGXTEntry("BLIP_474");
                    break;
                case eBuildingType.Facility: // animation
                    buildingBlip.Sprite = IsOwnedByPlayer ? 590 : 375;
                    buildingBlip.Name = IsOwnedByPlayer ? Game.GetGXTEntry("BLIP_590") : Game.GetGXTEntry("BLIP_474");
                    break;
            }

            blipManager.AddBlip(buildingBlip);

            if (IsOwnedByPlayer && !BuildingType.Equals(eBuildingType.Garage))
            {
                garageBlip.Positions.Add(GarageCarEnterance.ToPosition());
                garageBlip.IsShortRange = true;
                garageBlip.Sprite = 357;
                garageBlip.Name = Game.GetGXTEntry("BLIP_357");
                garageBlip.Category = 11;

                blipManager.AddBlip(garageBlip);
            }
        }

        internal void SetAsGarage()
        {
            Enterance = new Quaternion(GarageCarEnterance.X, GarageCarEnterance.Y, GarageCarEnterance.Z, 0F);
            Exit = Quaternion.Zero;
            Lobby = Quaternion.Zero;
            EnteranceCamera1 = null;
            EnteranceCamera2 = null;
            BuildingType = eBuildingType.Garage;
            FrontDoor = eFrontDoor.NoDoor;
            Door1 = null;
            Door2 = null;
            GarageDoor = eFrontDoor.NoDoor;
            Door3 = null;
            GarageWaypoint = Quaternion.Zero;
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

        public async void CreateForSaleSign()
        {
            if (_propForSaleSign is not null) return;
            Model propModel = SaleSign.Model;

            while (!propModel.IsLoaded)
            {
                await propModel.Request(100);
            }

            int propHandle = CreateObject(propModel.Hash, SaleSign.Position.X, SaleSign.Position.Y, SaleSign.Position.Z, false, true, false);

            if (DoesEntityExist(propHandle))
            {
                _propForSaleSign = new Prop(propHandle);
                _propForSaleSign.IsPersistent = true;
                _propForSaleSign.IsPositionFrozen = true;
                _propForSaleSign.Heading = SaleSign.Position.W;
            }

            propModel.MarkAsNoLongerNeeded();
        }

        public void DeleteForSaleSign()
        {
            if (_propForSaleSign?.Exists() ?? false)
                _propForSaleSign.Dispose();
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
            PluginManager.Instance.AttachTickHandler(OnDisableExteriorAsync);
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
            PluginManager.Instance.DetachTickHandler(OnDisableExteriorAsync);
        }

        public async Task OnDisableExteriorAsync()
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
