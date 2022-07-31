using CitizenFX.Core.UI;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Data;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Models;
using NativeUI;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    public class PropertyManager : Manager<ParticleManager>
    {
        MenuPool menuPool => PluginManager.MenuPool;

        public override async void Begin()
        {
            await Session.Loading();
            // BuildingData.Init();
        }

        // [TickHandler]
        private async Task OnPropertyManagerTickAsync()
        {
            BuildingData.SpawnForSaleSignsAndLockDoors();
            Ped ped = Game.PlayerPed;

            foreach(Building building in BuildingData.Buildings)
            {
                if (building.IsCloseToSaleSign)
                {
                    if (menuPool.IsAnyMenuOpen()) return;
                    if (ped.IsInVehicle()) return;

                    switch(building.BuildingType)
                    {
                        case eBuildingType.Apartment:
                        case eBuildingType.Clubhouse:
                            Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MP_PROP_PUR0"));
                            break;
                        case eBuildingType.Garage:
                        case eBuildingType.Hanger:
                            Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MP_PROP_PUR1"));
                            break;
                        case eBuildingType.Office:
                        case eBuildingType.Bunker:
                        case eBuildingType.Nightclub:
                        case eBuildingType.Warehouse:
                            Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MP_PROP_OFF_BUY"));
                            break;
                    }

                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        await ScreenInterface.FadeOut();
                        Game.PlayerPed.FadeOut();
                        Game.PlayerPed.IsPositionFrozen = true;
                        building.OpenBuyMenu();
                        World.RenderingCamera = World.CreateCamera(building.Camera.Position, building.Camera.Rotation, building.Camera.FieldOfView);
                        Cache.Player.DisableHud();
                        await ScreenInterface.FadeIn();
                    }
                }
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
