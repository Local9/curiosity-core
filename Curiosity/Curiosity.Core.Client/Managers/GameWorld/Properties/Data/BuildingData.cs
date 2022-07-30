using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Models;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Data
{
    internal static class BuildingData
    {
        public static List<Building> Buildings = new();
        static bool _saleSignsCreated;
        const string PROP_SALE_SIGN = "prop_forsale_dyn_01";

        public static async void Init()
        {
            await Session.Loading();

            Apartment _ThreeAltaStreet10 = new();
            _ThreeAltaStreet10.Id = 5;
            _ThreeAltaStreet10.Name = "MP_PROP_5";
            _ThreeAltaStreet10.Description = "MP_PROP_5DES";
            _ThreeAltaStreet10.Price = 217000;
            _ThreeAltaStreet10.Bed = new Quaternion(-260.0051f, -949.0003f, 70.02404f, 0f);
            _ThreeAltaStreet10.DoorPosition = new Quaternion(-264.7441f, -969.7109f, 76.23132f, 0f);
            _ThreeAltaStreet10.Door = new Door("hei_v_ilev_fh_heistdoor2", new Vector3(-264.6448f, -967.7161f, 77.34515f));
            _ThreeAltaStreet10.Enterance = new Quaternion(-262.9719f, -965.5146f, 76.23132f, 0f);
            _ThreeAltaStreet10.Exit = new Quaternion(-264.0814f, -967.5364f, 76.23132f, 0f);
            _ThreeAltaStreet10.Wardrobe = new Quaternion(-265.4738f, -947.6708f, 70.03869f, 160.6728f);
            _ThreeAltaStreet10.GarageFilePath = "3_alta_st_apt_10";
            _ThreeAltaStreet10.EnteranceCamera = new BuildingCamera(new Vector3(-262.6027f, -966.4926f, 77.74511f), new Vector3(0.1007454f, -0.000003034903f, 134.0962f), 50.0f);
            _ThreeAltaStreet10.ExitCamera = new BuildingCamera(new Vector3(-263.5748f, -969.6506f, 77.6988f), new Vector3(-0.05672785f, -0.0000001992695f, 10.8675f), 50.0f);

            Apartment _ThreeAltaStreet57 = new();
            _ThreeAltaStreet57.Id = 6;
            _ThreeAltaStreet57.Name = "MP_PROP_6";
            _ThreeAltaStreet57.Description = "MP_PROP_6DES";
            _ThreeAltaStreet57.Price = 223000;
            _ThreeAltaStreet57.Bed = new Quaternion(-284.4262f, -958.5359f, 86.3036f, 0f);
            _ThreeAltaStreet57.DoorPosition = new Quaternion(-278.4184f, -937.9305f, 91.51087f, 0f);
            _ThreeAltaStreet57.Door = new Door("hei_v_ilev_fh_heistdoor2", new Vector3(-278.5412f, -940.6227f, 92.62472f));
            _ThreeAltaStreet57.Enterance = new Quaternion(-281.0908f, -943.2817f, 92.5108f, 0f);
            _ThreeAltaStreet57.Exit = new Quaternion(-279.2097f, -940.9369f, 92.5108f, 0f);
            _ThreeAltaStreet57.Wardrobe = new Quaternion(-277.6365f, -960.4476f, 85.31431f, 345.1764f);
            _ThreeAltaStreet57.GarageFilePath = "3_alta_st_apt_57";
            _ThreeAltaStreet57.EnteranceCamera = new BuildingCamera(new Vector3(-280.7289f, -941.7155f, 93.1571f), new Vector3(-3.944601f, -0.000007488258f, -50.81424f), 50.0f);
            _ThreeAltaStreet57.ExitCamera = new BuildingCamera(new Vector3(-279.6175f, -938.8705f, 93.05049f), new Vector3(0.4648432f, -0.000004936041f, -163.8833f), 50.0f);
            _ThreeAltaStreet57.IsOwnedByPlayer = true;

            Building _ThreeAltaStreet = new();
            _ThreeAltaStreet.Name = "3 Alta St";
            _ThreeAltaStreet.Enterance = new Quaternion(-259.8061f, -969.4397f, 30.21999f, 70.91596f);
            _ThreeAltaStreet.Exit = new Quaternion(-261.1243f, -972.8566f, 30.21996f, 203.0815f);
            _ThreeAltaStreet.Lobby = new Quaternion(-263.679f, -966.7826f, 30.22428f, 204.0962f);
            _ThreeAltaStreet.Camera = new BuildingCamera(new Vector3(-215.2378f, -1071.639f, 32.85828f), new Vector3(22.62831f, 0f, 26.93762f), 50.0f);
            _ThreeAltaStreet.EnteranceCamera1 = new BuildingCamera(new Vector3(-261.2893f, -985.9326f, 34.31419f), new Vector3(-15.94485f, 0, 0.3253375f), 50.0f);
            _ThreeAltaStreet.EnteranceCamera2 = new BuildingCamera(new Vector3(-261.2893f, -985.9326f, 34.31419f), new Vector3(65.94485f, 0, 0.3253375f), 50.0f);
            _ThreeAltaStreet.GarageCamera1 = new BuildingCamera(new Vector3(-288.8877f, -994.0138f, 24.12381f), new Vector3(-3.488037f, -0.0000001069198f, -100.3268f), 50.0f); // Garage Camera
            _ThreeAltaStreet.GarageCamera2 = new BuildingCamera(new Vector3(-298.3831f, -990.7642f, 24.12381f), new Vector3(-3.879195f, -0.00000001337085f, -87.55238f), 50.0f); // Garage Camera
            _ThreeAltaStreet.BuildingType = eBuildingType.Apartment;
            _ThreeAltaStreet.ExteriorIndex = 5;
            _ThreeAltaStreet.FrontDoor = eFrontDoor.DoubleDoors;
            _ThreeAltaStreet.Door1 = new Door("hei_prop_dt1_20_mph_door_l", new Vector3(-263.461f, -970.5215f, 31.60709f)); // Front Door Left
            _ThreeAltaStreet.Door2 = new Door("hei_prop_dt1_20_mph_door_r", new Vector3(-260.6575f, -969.2133f, 31.60706f)); // Front Door Right
            _ThreeAltaStreet.Door3 = new Door("hei_prop_dt1_20_mp_gar2", new Vector3(-282.5465f, -995.163f, 24.68051f)); // Garage Door
            _ThreeAltaStreet.SaleSign = new SaleSign(PROP_SALE_SIGN, new Quaternion(-252.6184f, -970.720764f, 30.22f, -20.0f));
            _ThreeAltaStreet.GarageCarEnterance = new Quaternion(-279.7589f, -995.9545f, 24.5305f, 74.48383f);
            _ThreeAltaStreet.GarageCarExit = new Quaternion(-271.5633f, -999.2233f, 26.0224f, 249.66f);
            _ThreeAltaStreet.GarageFootEnterance = new Quaternion(-279.7421f, -992.0921f, 23.30595f, 74.48383f);
            _ThreeAltaStreet.GarageFootExit = new Quaternion(-286.7632f, -993.5939f, 23.13706f, 239.0284f);
            _ThreeAltaStreet.GarageType = eGarageType.Ten;
            _ThreeAltaStreet.GarageDoor = eFrontDoor.StandardDoor;
            _ThreeAltaStreet.GarageWaypoint = new Quaternion(-292.5203f, -991.2855f, 23.47978f, 250.2265f);
            _ThreeAltaStreet.Apartments.Add(_ThreeAltaStreet10);
            _ThreeAltaStreet.Apartments.Add(_ThreeAltaStreet57);

            Buildings.Add(_ThreeAltaStreet); // Move all of this into a JSON file

            Apartment _0120MurrietaHeightsGrg = new();
            _0120MurrietaHeightsGrg.Id = 24;
            _0120MurrietaHeightsGrg.Name = "MP_PROP_24";
            _0120MurrietaHeightsGrg.Description = "MP_PROP_24DES";
            _0120MurrietaHeightsGrg.Price = 150000;
            _0120MurrietaHeightsGrg.GarageFilePath = "0120_murrieta_heights";
            _0120MurrietaHeightsGrg.SetAsGarage();

            Building _0120MurrietaHeights = new();
            _0120MurrietaHeights.Name = "0120 Murrieta Heights";
            _0120MurrietaHeights.GarageCarEnterance = new Quaternion(966.7083F, -1019.782F, 40.12651F, 0f);
            _0120MurrietaHeights.GarageFootEnterance = new Quaternion(963.7991F, -1022.556F, 39.84747F, 88.24952F);
            _0120MurrietaHeights.GarageCarExit = new Quaternion(970.2502F, -1019.784F, 40.18027F, 270.5528F);
            _0120MurrietaHeights.Camera = new BuildingCamera(new Vector3(979.8295F, -1042.648F, 45.68815F), new Vector3(0.3886027F, -0.00000005336208F, 48.74495F), 50.0F);
            _0120MurrietaHeights.GarageType = eGarageType.Ten;
            _0120MurrietaHeights.SaleSign = new SaleSign(PROP_SALE_SIGN, new Quaternion(965.572F, -1011.164F, 40.04047F, 0F));
            _0120MurrietaHeights.SetAsGarage();
            _0120MurrietaHeights.Apartments.Add(_0120MurrietaHeightsGrg);

            Buildings.Add(_0120MurrietaHeights);

            foreach (Building building in Buildings)
            {
                building.CreateBuilding();
            }
        }

        public static void SpawnForSaleSignsAndLockDoors()
        {
            try
            {
                if (_saleSignsCreated) return;

                foreach (var bd in Buildings)
                {
                    bd.CreateForSaleSign();

                    switch (bd.FrontDoor)
                    {
                        case eFrontDoor.DoubleDoors:
                            bd.Door1.Lock();
                            bd.Door2.Lock();
                            break;

                        case eFrontDoor.StandardDoor:
                            bd.Door1.Lock();
                            break;
                    }

                    if (bd.GarageDoor == eFrontDoor.StandardDoor)
                        bd.Door3.Lock();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message} {ex.StackTrace}");
            }
            finally
            {
                _saleSignsCreated = true;
            }
        }
    }
}
