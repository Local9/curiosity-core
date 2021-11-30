using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Environment.Data;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Police;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Managers
{
    public class PoliceManager : Manager<PoliceManager>
    {
        ServerConfigManager serverConfigManager => ServerConfigManager.GetModule();

        private const int ALL_SERVER_ID = 0;
        Dictionary<int, DateTime> playerCullingReset = new();

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

            EventSystem.Attach("police:get:suspect:tickets", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                return await Database.Store.PoliceDatabase.GetTickets(curiosityUser.Character.CharacterId);
            }));

            EventSystem.Attach("police:suspect:jailed", new AsyncEventCallback(async metadata => {

                int suspectServerId = metadata.Find<int>(0);

                Player player = PluginManager.PlayersList[suspectServerId];
                SetEntityDistanceCullingRadius(player.Character.Handle, 0f); // reset culling
                player.State.Set(StateBagKey.PLAYER_IS_WANTED, false, true); // cannot want a dead person
                player.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 0, true);

                // log jail & reward officers
                // cut ticket cost in half

                return null;
            }));

            EventSystem.Attach("police:playerKilledPlayer", new AsyncEventCallback(async metadata => {

                int attackerServerId = metadata.Find<int>(0);
                int victimServerId = metadata.Find<int>(1);
                bool isMeleeDamage = metadata.Find<bool>(2);
                uint weaponInfoHash = metadata.Find<uint>(3);
                int damageTypeFlag = metadata.Find<int>(4);

                Player attacker = PluginManager.PlayersList[attackerServerId];
                Player victim = PluginManager.PlayersList[victimServerId];

                bool victimIsWanted = victim.State.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;
                bool attackerIsOfficer = (ePlayerJobs)(attacker.State.Get(StateBagKey.PLAYER_JOB) ?? 0) == ePlayerJobs.POLICE_OFFICER;

                if (attackerIsOfficer)
                {
                    if (victimIsWanted)
                    {
                        SetEntityDistanceCullingRadius(victim.Character.Handle, 0f); // reset culling
                        victim.State.Set(StateBagKey.PLAYER_IS_WANTED, false, true); // cannot want a dead person
                        victim.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 0, true);
                    }
                }

                Vector3 victimPosition = victim.Character.Position;
                Vector3 attackerPosition = attacker.Character.Position;

                float distanceTotal = Vector3.Distance(victimPosition, attackerPosition) / 1000f;
                float distanceFeet = distanceTotal * 5280f;

                string weaponName = DeathHash.CauseOfDeath[(int)weaponInfoHash];

                Dictionary<string, string> tableRows = new Dictionary<string, string>();
                tableRows.Add("Attacker", attacker.Name);
                tableRows.Add("Victim", victim.Name);
                tableRows.Add("Weapon", weaponName);

                if (distanceFeet > 100f)
                {
                    tableRows.Add($"Distance", $"{distanceFeet}ft");
                }

                string notificationTable = CreateBasicNotificationTable("Player Killed", tableRows);
                SendNotification(ALL_SERVER_ID, notificationTable);

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
                        em.error = "Player not found.";
                        goto RETURN_MESSAGE;
                    }

                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    Player player = PluginManager.PlayersList[metadata.Sender];

                    int speed = metadata.Find<int>(0);
                    int speedLimit = metadata.Find<int>(1);
                    // bool informPolice = metadata.Find<bool>(2);
                    int vehicleNetId = metadata.Find<int>(3);
                    string street = metadata.Find<string>(4);
                    string direction = metadata.Find<string>(5);
                    
                    // get Vehicle
                    int vehicleHandle = NetworkGetEntityFromNetworkId(vehicleNetId);
                    Vehicle vehicle = new Vehicle(vehicleHandle);

                    bool informPolice = (speed - speedLimit) > serverConfigManager.PoliceSpeedLimitWarning;

                    if (vehicle is null)
                    {
                        em.error = "Vehicle not found.";
                        goto RETURN_MESSAGE;
                    }

                    // add ticket to the database against the character/vehicle
                    int characterId = curiosityUser.Character.CharacterId;
                    int characterVehicleId = vehicle.State.Get(StateBagKey.VEH_ID); // mark in the Database
                    int costOfTicket = (int)((speed - speedLimit) * 50); // only charge for speed over the limit

                    bool success = await Database.Store.PoliceDatabase.InsertTicket(ePoliceTicketType.SPEEDING, characterId, characterVehicleId, costOfTicket, DateTime.UtcNow.AddDays(7), speed, speedLimit);

                    if (!success)
                    {
                        em.error = "Issue when trying to save ticket.";
                        goto RETURN_MESSAGE;
                    }

                    string numberPlate = GetVehicleNumberPlateText(vehicle.Handle);

                    string playerMsg = $"<table width=\"300\"><thead><tr><th colspan=\"2\">Caught Speeding ({costOfTicket:C0})</th></tr></thead>" +
                        $"<tbody><tr><td scope=\"row\" width=\"236\">" +
                        $"Location: {street}<br />Heading: {direction}<br />Make: MAKE_NAME<br />License Plate: {numberPlate}<br />Owner: {player.Name}<br />Speed: {speed} MPH";
                    playerMsg += $"</td><td><img src=\"./assets/img/icons/speedCameraWhite.png\" width=\"64\" /></td></tr></tbody></table>";

                    SendNotification(metadata.Sender, playerMsg, vehicleNetId: vehicle.NetworkId);

                    if (informPolice)
                    {
                        // wanted flag so police are not punished
                        player.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 1, true);

                        SetEntityDistanceCullingRadius(player.Character.Handle, 5000f); // make the player visible
                        playerCullingReset.Add(player.Character.Handle, DateTime.UtcNow.AddSeconds(15));
                        
                        string msg = $"<table width=\"300\"><thead><tr><th colspan=\"2\">Speeding Report</th></tr></thead>" +
                        $"<tbody><tr><td scope=\"row\" width=\"236\">" +
                        $"Last Location: {street}<br />Heading: {direction}<br />Make: MAKE_NAME<br />License Plate: {numberPlate}<br />Owner: {player.Name}<br />Speed: {speed} MPH" +
                        $"</td><td><img src=\"./assets/img/icons/speedCameraWhite.png\" width=\"64\" /></td></tr></tbody></table>";

                        SendNotification(serverId: ALL_SERVER_ID, message: msg, vehicleNetId: vehicle.NetworkId);
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

        [TickHandler]
        private async Task OnPlayerCullingReset()
        {
            if (playerCullingReset.Count == 0)
            {
                await BaseScript.Delay(5000);
            }
            else
            {
                foreach(KeyValuePair<int, DateTime> kvp in playerCullingReset.ToArray())
                {
                    if (kvp.Value < DateTime.UtcNow)
                    {
                        SetEntityDistanceCullingRadius(kvp.Key, 0f);
                        playerCullingReset.Remove(kvp.Key);
                    }
                }
            }
        }

        void SendNotification(int serverId = -1, string message = "", eNotification notification = eNotification.NOTIFICATION_INFO, int duration = 10000, int vehicleNetId = -1)
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

        string CreateBasicNotificationTable(string heading, Dictionary<string, string> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<table width=\"300\"><thead><tr><th colspan=\"2\">{heading}</th></tr></thead>");
            sb.Append($"<tbody>");

            foreach (KeyValuePair<string, string> row in rows)
            {
                sb.Append($"<tr><td>{row.Key}</td><td>{row.Value}</td></tr>");
            }

            sb.Append($"</tbody></table>");
            return sb.ToString();
        }
    }
}
