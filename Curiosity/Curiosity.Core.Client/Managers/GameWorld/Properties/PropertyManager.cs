﻿using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Data;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    public class PropertyManager : Manager<ParticleManager>
    {
        public override async void Begin()
        {
            BuildingData.Init();
        }

        [TickHandler]
        private async Task OnPropertyManagerTickAsync()
        {
            BuildingData.SpawnForSaleSignsAndLockDoors();
        }

        private async Task OnShowVehicleStatisticsTickAsync()
        {
            try
            {
                Vehicle[] closestVehicles = Game.PlayerPed.GetNearbyVehicles(20f);

                for (int i = 0; i < 10; i++)
                {
                    Scaleform scaleform = VehicleExtensions.CarStatScaleform;

                    switch (i)
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
            catch (Exception ex)
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
