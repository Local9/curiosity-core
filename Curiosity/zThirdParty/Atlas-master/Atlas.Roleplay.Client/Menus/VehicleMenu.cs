using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Menus
{
    public class VehicleMenu : Manager<VehicleMenu>
    {
        public bool SeatShuffle { get; set; }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var player = Cache.Player;

            if (player?.Entity != null)
            {
                var entity = Cache.Entity;

                if (API.IsPedSittingInAnyVehicle(entity.Id) && Game.IsControlJustPressed(0, Control.InteractionMenu) &&
                    !Game.IsControlPressed(0, Control.Sprint))
                {
                    OpenVehicleMenu();
                }

                if (API.IsPedInAnyVehicle(entity.Id, false))
                {
                    var decors = new EntityDecorModule
                    {
                        Id = API.GetVehiclePedIsIn(entity.Id, false)
                    };

                    if (!SeatShuffle)
                    {
                        if (API.GetPedInVehicleSeat(API.GetVehiclePedIsIn(entity.Id, false), 0) == entity.Id)
                        {
                            if (API.GetIsTaskActive(entity.Id, 165))
                            {
                                API.SetPedIntoVehicle(entity.Id, API.GetVehiclePedIsIn(entity.Id, false), 0);
                            }
                        }
                    }

                    var vehicleOff = decors.GetBoolean("Vehicle.Engine.Off");

                    if (vehicleOff)
                    {
                        API.SetVehicleEngineOn(decors.Id, false, true, false);
                    }
                }
            }

            await Task.FromResult(0);
        }

        private void OpenVehicleMenu()
        {
            new Menu("Fordonsmeny")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("engine_toggle", "Sätt på/av motor"),
                    new MenuItem("swap_seats", "Ändra säte"),
                    new MenuItem("doors", "Dörrar"),
                    new MenuItem("windows", "Veva upp/ner rutor")
                },
                Callback = async (menu, item, operation) =>
                {
                    var entity = Cache.Entity;

                    if (operation.Type != MenuOperationType.Select) return;
                    if (!API.IsPedSittingInAnyVehicle(entity.Id)) return;

                    var vehicleId = API.GetVehiclePedIsIn(entity.Id, false);
                    var decors = new EntityDecorModule
                    {
                        Id = vehicleId
                    };

                    if (item.Seed == "windows")
                    {
                        var windowIndex = 0;

                        for (var i = 0; i < 3; i++)
                        {
                            if (API.GetPedInVehicleSeat(vehicleId, i) != entity.Id) continue;

                            windowIndex = i;

                            break;
                        }

                        var rolledDown = decors.GetBoolean("Vehicle.Windows.RolledDown");

                        if (!rolledDown)
                        {
                            API.RollDownWindow(vehicleId, windowIndex);
                        }
                        else
                        {
                            API.RollUpWindow(vehicleId, windowIndex);
                        }

                        decors.Set("Vehicle.Windows.RolledDown", !rolledDown);
                    }
                    else if (item.Seed == "swap_seats")
                    {
                        SeatShuffle = true;

                        await BaseScript.Delay(5000);

                        SeatShuffle = false;
                    }
                    else if (item.Seed == "engine_toggle")
                    {
                        var toggle = !API.IsVehicleEngineOn(vehicleId);

                        API.SetVehicleEngineOn(vehicleId, toggle, false, false);

                        decors.Set("Vehicle.Engine.Off", !toggle);
                    }
                    else if (item.Seed == "doors")
                    {
                        menu.Hide();

                        OpenDoorMenu(vehicleId);
                    }
                }
            }.Commit();
        }

        private void OpenDoorMenu(int vehicleId)
        {
            var elements = new List<MenuItem>();

            for (var i = 0; i < API.GetNumberOfVehicleDoors(vehicleId); i++)
            {
                elements.Add(new MenuItem($"door_index_{i}", $"Dörr #{i + 1}", i));
            }

            new Menu("Dörrar")
            {
                Items = elements,
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    var entity = Cache.Entity;

                    if (!API.IsPedSittingInAnyVehicle(entity.Id)) return;

                    var index = (int)item.Metadata[0];

                    vehicleId = API.GetVehiclePedIsIn(entity.Id, false);

                    var angle = API.GetVehicleDoorAngleRatio(vehicleId, index);
                    var closed = Math.Abs(angle) < 1;

                    if (!closed) API.SetVehicleDoorShut(vehicleId, index, false);
                    else API.SetVehicleDoorOpen(vehicleId, index, false, false);
                }
            }.Commit();
        }
    }
}