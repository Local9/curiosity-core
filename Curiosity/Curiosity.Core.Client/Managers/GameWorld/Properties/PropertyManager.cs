using Curiosity.Core.Client.Extensions;
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
            _ThreeAltaStreet10.GarageFile = "3_alta_st_apt_10";
            _ThreeAltaStreet10.EnteranceCamera = new BuildingCamera(new Vector3(-262.6027f, -966.4926f, 77.74511f), new Vector3(0.1007454f, -0.000003034903f, 134.0962f), 50.0f);
            _ThreeAltaStreet10.ExitCamera = new BuildingCamera(new Vector3(-263.5748f, -969.6506f, 77.6988f), new Vector3(-0.05672785f, -0.0000001992695f, 10.8675f), 50.0f);

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
            _ThreeAltaStreet.Garage = new();
            _ThreeAltaStreet.Garage.CarEnterance = new Quaternion(-279.7589f, -995.9545f, 24.5305f, 74.48383f);
            _ThreeAltaStreet.Garage.CarExit = new Quaternion(-271.5633f, -999.2233f, 26.0224f, 249.66f);
            _ThreeAltaStreet.Garage.FootEnterance = new Quaternion(-279.7421f, -992.0921f, 23.30595f, 74.48383f);
            _ThreeAltaStreet.Garage.FootExit = new Quaternion(-286.7632f, -993.5939f, 23.13706f, 239.0284f);
            _ThreeAltaStreet.Garage.GarageType = eGarageType.Ten;
            _ThreeAltaStreet.Garage.Door = eFrontDoor.StandardDoor;
            _ThreeAltaStreet.Garage.Waypoint = new Quaternion(-292.5203f, -991.2855f, 23.47978f, 250.2265f);
            _ThreeAltaStreet.Apartments.Add(_ThreeAltaStreet10);

            _buildings.Add(_ThreeAltaStreet); // Move all of this into a JSON file

            foreach(Building building in _buildings)
            {
                    building.CreateBuilding();
            }
        }

        bool _transition;

        private async Task OnShowVehicleStatisticsTickAsync()
        {
            try
            {
                Vehicle[] closestVehicles = Game.PlayerPed.GetNearbyVehicles(20f);

                for(int i = 0; i < 10; i++)
                {
                    Scaleform scaleform = VehicleExtensions.CarStatScaleform;

                    switch(i)
                    {
                        case 0:
                            scaleform = VehicleExtensions.CarStatScaleform;
                            break;
                        case 1:
                            scaleform = VehicleExtensions.CarStatScaleform2;
                            break;
                        case 2:
                            scaleform = VehicleExtensions.CarStatScaleform3;
                            break;
                        case 3:
                            scaleform = VehicleExtensions.CarStatScaleform4;
                            break;
                        case 4:
                            scaleform = VehicleExtensions.CarStatScaleform5;
                            break;
                        case 5:
                            scaleform = VehicleExtensions.CarStatScaleform6;
                            break;
                        case 6:
                            scaleform = VehicleExtensions.CarStatScaleform7;
                            break;
                        case 7:
                            scaleform = VehicleExtensions.CarStatScaleform8;
                            break;
                        case 8:
                            scaleform = VehicleExtensions.CarStatScaleform9;
                            break;
                        case 9:
                            scaleform = VehicleExtensions.CarStatScaleform10;
                            break;
                    }

                    closestVehicles[i].DrawCarStats(scaleform);
                }

            }
            catch(Exception ex)
            {

            }
        }

        private async Task EnterApartment()
        {
            //_transition = true;
            //Audio.PlaySoundAt(Game.PlayerPed.Position, "DOOR_BUZZ", "MP_PLAYER_APARTMENT");
            //await building.PlayEnterApartmentCamera(3000, true, true, CameraShake.Hand, 0.4f);
            //Apartment apartment = building.Apartments[0];
            //apartment.SetInteriorActive();
            //Game.PlayerPed.Position = apartment.Enterance.AsVector();
            //// DOOR SCRIPT
            //await apartment.PlayEnteranceCutscene();
            //World.DestroyAllCameras();
            //World.RenderingCamera = null;
            //_transition = false;
        }
    }
}
