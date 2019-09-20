using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Player.Client.net.Classes.Environment
{
    class PlayerKillTracker
    {
        public static bool IsPlayerDead = false;
        public static bool HasPlayerDied = false;
        public static int? DiedAt = null;


        static Client client = Client.GetInstance();
        static public void Init()
        {
            client.RegisterTickHandler(OnPlayerDeathTracker);
        }

        static async Task OnPlayerDeathTracker()
        {
            await Client.Delay(0);

            int playerServerId = Client.PlayerServerId;
            int playerPedId = Client.PlayerPedId;

            if (NetworkIsPlayerActive(playerServerId))
            {
                if (IsPedFatallyInjured(playerPedId) && !IsPlayerDead)
                {
                    IsPlayerDead = true;
                    if (DiedAt == null)
                    {
                        DiedAt = GetGameTimer();
                    }

                    uint killerWeaponHash = 0;
                    int killerId = NetworkGetEntityKillerOfPlayer(playerPedId, ref killerWeaponHash);
                    int killerEntityType = GetEntityType(killerId);
                    int killerPedId = GetPlayerPed(killerId);
                    int killerType = -1;
                    bool killerInVehicle = false;
                    string killerVehicleName = string.Empty;

                    if (killerEntityType == 1)
                    {
                        killerType = GetPedType(killerId);
                        if (IsPedInAnyVehicle(killerPedId, false))
                        {
                            killerInVehicle = true;
                            killerVehicleName = GetDisplayNameFromVehicleModel((uint)GetEntityModel(GetVehiclePedIsUsing(killerPedId)));
                        }
                        else
                        {
                            killerInVehicle = false;
                        }
                    }

                    if (killerId != Client.players[playerServerId].Handle && NetworkIsPlayerActive(killerId))
                    {
                        killerId = GetPlayerServerId(killerId);
                    }
                    else
                    {
                        killerId = -1;
                    }

                    Vector3 positionOfDeath = Game.PlayerPed.Position;

                    Client.TriggerServerEvent("curiosity:Server:Player:LogDeath", killerId, killerType, killerInVehicle, killerVehicleName, positionOfDeath.X, positionOfDeath.Y, positionOfDeath.Z);
                    HasPlayerDied = true;
                }
                else if (!IsPedFatallyInjured(playerPedId))
                {
                    IsPlayerDead = false;
                    DiedAt = null;
                }
            }
        }
    }
}
