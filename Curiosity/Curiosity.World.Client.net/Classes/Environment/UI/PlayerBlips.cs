﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Classes.Environment.UI
{
    static class PlayerBlips
    {
        static internal Client client = Client.GetInstance();

        static internal IEnumerable<CitizenFX.Core.Player> BlipPlayersList;
        static internal bool IsRadarExtended = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnControlPressed);
            client.RegisterEventHandler("curiosity:Client:UI:SetMapScale", new Action<bool>(SetMapScale));

            client.RegisterTickHandler(OnTickShowPlayerBlips);
        }

        static internal bool ShouldShowBlip(CitizenFX.Core.Player player)
        {
            if (player == null) return false;

            if (!API.NetworkIsPlayerActive(player.Handle) && Game.Player.Handle == player.Handle) return false;

            bool isCurrentPlayer = (Game.Player == player);
            if (!isCurrentPlayer)
                return true;
            return false;
        }

        static void SetMapScale(bool scale)
        {
            if (scale)
            {
                API.SetResourceKvp("curiosity:minimap:scale", "large");
            }
            else
            {
                API.SetResourceKvp("curiosity:minimap:scale", "small");
            }
        }

        static internal async Task OnControlPressed()
        {
            if (ControlHelper.IsControlPressed(Control.MultiplayerInfo))
            {
                API.SetRadarBigmapEnabled(true, false);
                IsRadarExtended = true;
            }
            else
            {
                if (IsRadarExtended)
                {
                    await Client.Delay(5000);
                }
                API.SetRadarBigmapEnabled(API.GetResourceKvpString("curiosity:minimap:scale") == "large", false);
                IsRadarExtended = false;
            }
        }

        static public async Task OnTickShowPlayerBlips()
        {
            try
        {
                BlipPlayersList = Client.playerList.Where(ShouldShowBlip);
                List<CitizenFX.Core.Player> playerList = BlipPlayersList.ToList();
                playerList.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position)).Select((player, rank) => new { player, rank }).ToList().ForEach(async p => await ShowBlip(p.player));
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR PlayerNames: {ex.Message}");
            }
        }

        static internal async Task ShowBlip(CitizenFX.Core.Player player)
        {
            int playerHandle = player.Handle;

            if (!API.NetworkIsPlayerActive(player.Handle) && Game.Player.Handle == playerHandle) return;

            int blip = API.GetBlipFromEntity(player.Character.Handle);
            int playerCharacterHandle = player.Character.Handle;

            int wantedLevel = API.GetPlayerWantedLevel(playerHandle);
            int playerGamerTag = API.CreateMpGamerTag(playerHandle, player.Name, false, false, "", 0);

            if (!API.NetworkIsPlayerActive(player.Handle))
            {
                API.SetMpGamerTagVisibility(playerGamerTag, 0, false);
                if (API.DoesBlipExist(blip))
                {
                    API.SetBlipAlpha(blip, 0);
                }
            }
            else
            {

                if (wantedLevel > 0)
                {
                    API.SetMpGamerTagVisibility(playerGamerTag, 7, true);
                    API.SetMpGamerTagWantedLevel(playerGamerTag, wantedLevel);
                }
                else
                {
                    API.SetMpGamerTagVisibility(playerGamerTag, 7, false);
                }

                if (API.NetworkIsPlayerTalking(playerHandle))
                {
                    API.SetMpGamerTagVisibility(playerGamerTag, 9, true);
                }
                else
                {
                    API.SetMpGamerTagVisibility(playerGamerTag, 9, false);
                }

                if (!API.DoesBlipExist(blip))
                {
                    blip = API.AddBlipForEntity(playerCharacterHandle);
                    API.SetBlipSprite(blip, 1);
                    API.ShowHeadingIndicatorOnBlip(blip, true);
                }
                else
                {
                    int vehicleHandle = API.GetVehiclePedIsIn(playerCharacterHandle, false);
                    BlipSprite currentBlipSprite = (BlipSprite)API.GetBlipSprite(blip);

                    if (API.IsEntityDead(playerCharacterHandle))
                    {
                        if (currentBlipSprite != BlipSprite.Dead)
                        {
                            API.SetBlipSprite(blip, (int)BlipSprite.Dead);
                            API.ShowHeadingIndicatorOnBlip(blip, false);
                        }
                    }
                    else if (vehicleHandle > 0)
                    {
                        VehicleClass vehicleClass = (VehicleClass)API.GetVehicleClass(vehicleHandle);
                        int vehModel = API.GetEntityModel(vehicleHandle);

                        if (vehicleClass == VehicleClass.Helicopters)
                        {
                            if (currentBlipSprite != BlipSprite.HelicopterAnimated)
                            {
                                API.SetBlipSprite(blip, (int)BlipSprite.HelicopterAnimated);
                                API.ShowHeadingIndicatorOnBlip(blip, false);
                            }
                        }
                        else if (vehicleClass == VehicleClass.Planes)
                        {
                            if (currentBlipSprite != BlipSprite.Plane)
                            {
                                API.SetBlipSprite(blip, (int)BlipSprite.Plane);
                                API.ShowHeadingIndicatorOnBlip(blip, false);
                            }
                        }
                        else if (vehicleClass == VehicleClass.Boats)
                        {
                            if (currentBlipSprite != BlipSprite.Boat)
                            {
                                API.SetBlipSprite(blip, (int)BlipSprite.Boat);
                                API.ShowHeadingIndicatorOnBlip(blip, false);
                            }
                        }
                        else
                        {
                            if (currentBlipSprite != BlipSprite.Standard)
                            {
                                if (API.IsVehicleSirenOn(vehicleHandle))
                                {
                                    API.SetBlipSprite(blip, (int)BlipSprite.PoliceCarDot);
                                }
                                else
                                {
                                    API.SetBlipSprite(blip, (int)BlipSprite.Standard);
                                }

                                API.ShowHeadingIndicatorOnBlip(blip, true);
                            }
                        }

                        int passengers = API.GetVehicleNumberOfPassengers(vehicleHandle);
                        if (passengers > 0)
                        {
                            if (!API.IsVehicleSeatFree(vehicleHandle, -1))
                            {
                                passengers = passengers + 1;
                            }

                            API.ShowNumberOnBlip(blip, passengers);
                        }
                        else
                        {
                            API.HideNumberOnBlip(blip);
                        }
                    }
                    else
                    {
                        API.HideNumberOnBlip(blip);
                        if (currentBlipSprite != BlipSprite.Standard)
                        {
                            API.SetBlipSprite(blip, (int)BlipSprite.Standard);
                            API.ShowHeadingIndicatorOnBlip(blip, true);
                        }
                    }

                    API.SetBlipRotation(blip, (int)Math.Ceiling(API.GetEntityHeading(vehicleHandle)));
                    API.SetBlipNameToPlayerName(blip, playerHandle);
                    API.SetBlipScale(blip, 0.85f);
                    API.SetBlipCategory(blip, 7);
                    API.SetBlipPriority(blip, 11);

                    if (API.IsPauseMenuActive())
                    {
                        API.SetBlipAlpha(blip, 255);
                    }
                    else
                    {
                        Vector3 characterPosition = player.Character.Position;
                        Vector3 playerPosition = Game.PlayerPed.Position;

                        double distance = (Math.Floor(Math.Abs(Math.Sqrt(
                            (playerPosition.X - characterPosition.X) *
                            (playerPosition.X - characterPosition.X) +
                            (playerPosition.Y - characterPosition.Y) *
                            (playerPosition.Y - characterPosition.Y))) / -1)) + 900;

                        if (distance < 0)
                        {
                            distance = 0;
                        }
                        else if (distance > 255)
                        {
                            distance = 255;
                        }

                        if (API.IsEntityVisible(playerCharacterHandle))
                        {
                            API.SetBlipAlpha(blip, (int)distance);
                        }
                        else
                        {
                            API.SetBlipAlpha(blip, 0);
                        }
                    }
                }
            }

            await Task.FromResult(0);
        }
    }
}
