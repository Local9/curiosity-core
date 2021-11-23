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
                    // check number of active officers
                    int activeOfficers = GetPlayersWhoArePolice().Count;
                    if (activeOfficers > 25)
                    {
                        SendNotification(metadata.Sender, $"We currently have 20 active officers, please try again later.");
                        return false;
                    }

                    int numberOfPlayers = PluginManager.PlayersList.Count();
                    float ratioOfCopsToPlayers = (activeOfficers / numberOfPlayers);

                    if (ratioOfCopsToPlayers > 0.5)
                    {
                        SendNotification(metadata.Sender, $"We currently have enough active officers, please try again later.");
                        return false;
                    }

                    curiosityUser.Job = ePlayerJobs.POLICE_OFFICER;
                    player.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUser.Job, true);

                    SendNotification(metadata.Sender, $"Welcome to the force.");

                    return true;
                }

                if (!activate)
                {
                    curiosityUser.Job = ePlayerJobs.UNEMPLOYED;
                    player.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUser.Job, true);

                    SendNotification(metadata.Sender, $"You have now left the police force.");

                    return false;
                }

                return false;
            }));

            EventSystem.Attach("police:suspect:jailed", new AsyncEventCallback(async metadata => {
                return null;
            }));

            EventSystem.Attach("police:suspect:killed", new AsyncEventCallback(async metadata => {

                int suspectServerId = metadata.Find<int>(0);

                Player player = PluginManager.PlayersList[suspectServerId];
                SetEntityDistanceCullingRadius(player.Character.Handle, 0f); // reset
                player.State.Set(StateBagKey.PLAYER_IS_WANTED, false, true);

                // log kill & reward officers (if kill is near a safe area, its not rewarded)

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

                    int speed = metadata.Find<int>(0);
                    int speedLimit = metadata.Find<int>(1);
                    bool informPolice = metadata.Find<bool>(2);
                    int vehicleNetId = metadata.Find<int>(3);
                    string street = metadata.Find<string>(4);
                    string direction = metadata.Find<string>(5);
                    
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
                    int characterVehicleId = vehicle.State.Get(StateBagKey.VEH_ID); // mark in the Database
                    // wanted flag so police are not punished
                    player.State.Set(StateBagKey.PLAYER_IS_WANTED, informPolice, true);
                    // ticket
                    int costOfTicket = (int)((speed - speedLimit) * 50); // only charge for speed over the limit
                    SetEntityDistanceCullingRadius(player.Character.Handle, 5000f); // make the player visible
                    // store in the database

                    if (informPolice)
                    {
                        string numberPlate = GetVehicleNumberPlateText(vehicle.Handle);
                        
                        string msg = $"<table width=\"300\"><thead><tr><th colspan=\"2\">Speeding Report</th></tr></thead>" +
                        $"<tbody><tr><td scope=\"row\" width=\"236\">" +
                        $"Location: {street}<br />Heading: {direction}<br />Make: MAKE_NAME<br />License Plate: {numberPlate}<br />Speed: {speed} MPH" +
                        $"</td><td><img src=\"./assets/img/icons/speedCameraWhite.png\" width=\"64\" /></td></tr></tbody></table>";

                        SendNotification(message: msg);
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

        void SendNotification(int serverId = 0, string message = "", eNotification notification = eNotification.NOTIFICATION_INFO, int duration = 10000, int vehicleNetId = -1)
        {
            if (serverId == -1) // all
            {
                EventSystem.SendAll("police:notify", notification, message, duration, vehicleNetId);
            }

            if (serverId > 0) // one
            {
                EventSystem.Send("police:notify", serverId, notification, message, duration, vehicleNetId);
            }

            if (serverId == 0) // job
            {
                foreach (int activeOfficerId in GetPlayersWhoArePolice())
                {
                    // Need to inform all of them the vehicle information
                    EventSystem.Send("police:notify", activeOfficerId, notification, message, duration, vehicleNetId);
                }
            }
        }

        List<int> GetPlayersWhoArePolice()
        {
            return PluginManager.ActiveUsers
                .Where(y => y.Value.Job == ePlayerJobs.POLICE_OFFICER)
                .Select(x => x.Key).ToList();
        }
    }
}
