using CitizenFX.Core.UI;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Data;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Models;
using NativeUI;
using System.Drawing;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    public class PropertyManager : Manager<ParticleManager>
    {
        MenuPool menuPool => PluginManager.MenuPool;

        public override async void Begin()
        {
            await Session.Loading();
            BuildingData.Init();
        }

        [TickHandler]
        private async Task OnPropertyManagerTickAsync()
        {
            BuildingData.SpawnForSaleSignsAndLockDoors();
            Ped ped = Game.PlayerPed;

            foreach (Building building in BuildingData.Buildings)
            {
                if (building.IsCloseToSaleSign)
                {
                    if (menuPool.IsAnyMenuOpen()) return;
                    if (ped.IsInVehicle()) return;

                    switch (building.BuildingType)
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

                if (building.IsInRangetOfGarageEnterance(10.0f))
                {
                    Vector3 markerPos = building.GarageFootEnterance.AsVector();
                    Vector3 scale = Vector3.One;

                    if (Game.PlayerPed.IsInVehicle())
                    {
                        markerPos = building.GarageCarEnterance.AsVector();
                        markerPos -= new Vector3(0.0f, 0.0f, 0.5f);
                        scale = new Vector3(4f, 4f, 1f);
                    }

                    World.DrawMarker(MarkerType.VerticalCylinder, markerPos, Vector3.Zero, Vector3.Zero, scale, Color.FromArgb(255, 255, 255, 255));
                }

                if (building.IsInRangetOfGarageEnterance(5.0f))
                {
                    if (menuPool.IsAnyMenuOpen()) return;
                    Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MP_PROP_BUZZ1B"));
                    
                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Vehicle currentVehicle = Game.PlayerPed.CurrentVehicle;
                            // need to check player owns the vehicle
                            return;
                        }

                        await ScreenInterface.FadeOut();
                        Game.PlayerPed.FadeOut();
                        Game.PlayerPed.IsPositionFrozen = true;
                        building.OpenGarageMenu();
                        World.RenderingCamera = World.CreateCamera(building.Camera.Position, building.Camera.Rotation, building.Camera.FieldOfView);
                        Cache.Player.DisableHud();
                        await ScreenInterface.FadeIn();
                    }
                }
            }
        }
    }
}
