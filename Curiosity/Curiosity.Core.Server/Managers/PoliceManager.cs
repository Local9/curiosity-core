using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Managers
{
    public class PoliceManager : Manager<PoliceManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("police:job:state", new EventCallback(metadata => {

                Player player = PluginManager.PlayersList[metadata.Sender];
                if (player == null) return false;

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                bool activate = metadata.Find<bool>(0);

                if (activate)
                {
                    curiosityUser.Job = ePlayerJobs.POLICE_OFFICER;
                    player.State.Set(StateBagKey.PLAYER_JOB, curiosityUser.Job, true);
                    return true;
                }

                if (!activate)
                {
                    curiosityUser.Job = ePlayerJobs.UNEMPLOYED;
                    player.State.Set(StateBagKey.PLAYER_JOB, curiosityUser.Job, true);
                    return false;
                }

                return false;
            }));

            EventSystem.Attach("police:suspect:jailed", new AsyncEventCallback(async metadata => {
                return null;
            }));

            EventSystem.Attach("police:suspect:killed", new AsyncEventCallback(async metadata => {
                return null;
            }));

            EventSystem.Attach("police:report:murder", new AsyncEventCallback(async metadata => {
                return null;
            }));

            EventSystem.Attach("police:ticket:speeding", new AsyncEventCallback(async metadata => {
                ExportMessage em = new ExportMessage();
                
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    {
                        return null;
                    }

                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    Player player = PluginManager.PlayersList[metadata.Sender];

                    float speed = metadata.Find<float>(0);
                    float speedLimit = metadata.Find<float>(1);
                    bool informPolice = metadata.Find<bool>(2);
                    int vehicleNetId = metadata.Find<int>(3);
                    
                    // get Vehicle
                    int vehicleHandle = NetworkGetEntityFromNetworkId(vehicleNetId);
                    Vehicle vehicle = new Vehicle(vehicleHandle);

                    if (vehicle is null)
                    {
                        em.error = "Vehicle not found.";
                        goto RETURN_MESSAGE;
                    }

                    // add ticket to the database against the character/vehicle
                    int characterId = curiosityUser.Character.CharacterId;
                    int characterVehicleId = vehicle.State.Get(StateBagKey.VEH_ID);
                    // wanted flag so police are not punished
                    vehicle.State.Set(StateBagKey.VEH_IS_WANTED, informPolice, true);
                    player.State.Set(StateBagKey.PLAYER_IS_WANTED, informPolice, true);
                    // ticket
                    int costOfTicket = (int)((speed - speedLimit) * 50); // only charge for speed over the limit
                    SetEntityDistanceCullingRadius(player.Character.Handle, 5000f); // make the player visible
                    SetEntityDistanceCullingRadius(vehicle.Handle, 5000f); // make the vehicle visible

                    if (informPolice)
                    {
                        string numberPlate = GetVehicleNumberPlateText(vehicle.Handle);
                        string msg = $"";
                        InformPolice(msg, vehicleNetId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"police:ticket:speeding");
                    em.error = $"Error when invoicing speeding ticket.";
                }

            RETURN_MESSAGE:
                return em;
            }));
        }

        void InformPolice(string message, int vehicleNetworkId)
        {
            foreach(int serverId in GetPlayersWhoArePolice())
            {
                // Need to inform all of them the vehicle information
                EventSystem.Send("police:report:notify", serverId, message, vehicleNetworkId);
            }
        }

        List<int> GetPlayersWhoArePolice()
        {
            return PluginManager.ActiveUsers.Where(y => y.Value.Job == ePlayerJobs.POLICE_OFFICER).Select(x => x.Key).ToList();
        }
    }
}
