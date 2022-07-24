using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Environment.Data;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Police;
using Curiosity.Systems.Library.Utils;
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
        const long TWO_MINUTES = (1000 * 60) * 2;
        ServerConfigManager serverConfigManager => ServerConfigManager.GetModule();
        DiscordClient discordClient => DiscordClient.GetModule();

        private const int SEND_JOB_ONLY = 0;
        private const int MAX_NUMBER_OFFICERS = 20;
        private const int TIME_TIL_CULLING_RESET = (1000 * 10);
        private const int DB_POLICE_SKILL = 5;
        Dictionary<Player, long> playerCullingReset = new();
        const int CALL_SIGN_LENGTH = 4;

        public override void Begin()
        {
            EventSystem.Attach("police:job:state", new AsyncEventCallback(async metadata =>
            {

                Player player = PluginManager.PlayersList[metadata.Sender];
                if (player == null) return false;

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                bool activate = metadata.Find<bool>(0);

                if (activate && curiosityUser.Job != ePlayerJobs.POLICE_OFFICER)
                {
                    // check number of active officers
                    int activeOfficers = GetPlayersWhoArePolice().Count;
                    if (activeOfficers > MAX_NUMBER_OFFICERS)
                    {
                        SendNotification(metadata.Sender, $"We currently have 20 active officers, please try again later.");
                        return false;
                    }

                    int numberOfPlayers = PluginManager.PlayersList.Count();
                    double ratioOfCopsToPlayers = (activeOfficers / numberOfPlayers);

                    double ratio = 0.33f;

                    switch(numberOfPlayers)
                    {
                        case int n when n <= 5:
                            ratio = 0.5f;
                            break;
                        case int n when n <= 10:
                            ratio = 0.45f;
                            break;
                        default:
                            ratio = 0.33f;
                            break;
                    }

                    if (ratioOfCopsToPlayers > ratio && !curiosityUser.IsStaff)
                    {
                        SendNotification(metadata.Sender, $"We currently have enough active officers or there are not enough active players, please try again later.");
                        return false;
                    }

                    bool isPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                    if (isPassive)
                    {
                        SendNotification(metadata.Sender, $"Cannot join the force while passive.");
                        return false;
                    }

                    bool isWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                    if (isWanted)
                    {
                        SendNotification(metadata.Sender, $"Cannot join the force while wanted.");
                        return false;
                    }

                    bool isJailed = player.State.Get(StateBagKey.IS_JAILED) ?? false;
                    if (isJailed)
                    {
                        SendNotification(metadata.Sender, $"Cannot join the force while jailed.");
                        return false;
                    }

                    curiosityUser.Job = ePlayerJobs.POLICE_OFFICER;
                    await SetUserJobText(metadata.Sender, "Police Officer [PvP]");
                    player.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUser.Job, true);

                    SendNotification(metadata.Sender, $"Welcome to the force.");

                    return true;
                }

                if (!activate && curiosityUser.Job == ePlayerJobs.POLICE_OFFICER)
                {
                    curiosityUser.Job = ePlayerJobs.UNEMPLOYED;
                    curiosityUser.CurrentJob = "Unemployed";
                    curiosityUser.JobCallSign = string.Empty;
                    player.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUser.Job, true);

                    SendNotification(metadata.Sender, $"You have now left the police force.");

                    return false;
                }

                return false;
            }));

            EventSystem.Attach("police:suspect:ticket:get", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                return await Database.Store.PoliceDatabase.GetTickets(curiosityUser.Character.CharacterId);
            }));

            EventSystem.Attach("police:suspect:ticket:pay:overdue", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                List<PoliceTicket> tickets = await Database.Store.PoliceDatabase.GetTickets(curiosityUser.Character.CharacterId);
                List<PoliceTicket> cpTickets = new(tickets);
                ulong totalPaid = 0;
                ulong totalUnpaid = 0;

                foreach(PoliceTicket ticket in cpTickets)
                {
                    if (ticket.PaymentOverdue)
                    {
                        ulong ticketValue = (ulong)(ticket.TicketValue * 1.1);
                        if (curiosityUser.Character.Cash >= ticketValue)
                        {
                            bool updatedTicket = await Database.Store.PoliceDatabase.PayTicket(curiosityUser.Character.CharacterId, ticket.Id);
                            await BaseScript.Delay(0);
                            if (updatedTicket)
                            {
                                curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (long)ticketValue * -1);
                                totalPaid += ticketValue;
                            }
                            await BaseScript.Delay(10);
                        }

                        if (curiosityUser.Character.Cash < (ulong)ticket.TicketValue)
                        {
                            totalUnpaid += (ulong)ticket.TicketValue;
                        }
                    }
                }


                if (totalPaid > 0)
                    SendNotification(metadata.Sender, $"Paid ${totalPaid:N0} on Overdue tickets, with ${totalUnpaid:N0} remaining as you are low on funds. Please refresh your tickets to see the remaining.", eNotification.NOTIFICATION_SUCCESS);

                if (totalPaid == 0)
                    SendNotification(metadata.Sender, $"No outstanding tickets where paid.", eNotification.NOTIFICATION_SUCCESS);

                return null;
            }));

            EventSystem.Attach("police:suspect:ticket:pay", new AsyncEventCallback(async metadata =>
            {
                ExportMessage em = new();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    Logger.Error("police:suspect:ticket:pay => Player could not be found");
                    em.error = "Player not found";
                    return em;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                int ticketId = metadata.Find<int>(0);

                List<PoliceTicket> tickets = await Database.Store.PoliceDatabase.GetTickets(curiosityUser.Character.CharacterId);
                PoliceTicket policeTicket = null;

                foreach (PoliceTicket ticket in tickets)
                {
                    if (ticket.Id == ticketId)
                        policeTicket = ticket;
                }

                if (policeTicket is null)
                {
                    Logger.Error("police:suspect:ticket:pay => Invalid Ticket");
                    em.error = "Ticket not found, or has already been paid. Please refresh your tickets.";
                    return em;
                }

                if (curiosityUser.Character.Cash < (ulong)policeTicket.TicketValue)
                {
                    SendNotification(metadata.Sender, $"Not enough cash to pay.", eNotification.NOTIFICATION_WARNING);
                    Logger.Error("police:suspect:ticket:pay => Not enough cash to pay");
                    em.error = "Not enough cash to pay ticket.";
                    return em;
                }

                bool updatedTicket = await Database.Store.PoliceDatabase.PayTicket(curiosityUser.Character.CharacterId, ticketId);

                if (updatedTicket)
                {
                    SendNotification(metadata.Sender, $"Ticket Paid.", eNotification.NOTIFICATION_SUCCESS);
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, policeTicket.TicketValue * -1);
                    return em;
                }

                Logger.Error("police:suspect:ticket:pay => Failed to update ticket");
                em.error = "Failed to updated ticket.";
                return em;
            }));

            EventSystem.Attach("police:suspect:jailed", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    int suspectServerId = metadata.Find<int>(0);
                    Player player = PluginManager.PlayersList[suspectServerId];
                    if (player == null) return false;

                    Player officer = PluginManager.PlayersList[metadata.Sender];
                    if (officer == null) return false;

                    bool isPlayerJailed = player.State.Get(StateBagKey.IS_JAILED) ?? false;
                    if (isPlayerJailed) return false;

                    await SendSuspectToJail(suspectServerId, player);

                    // log jail & reward officers
                    // cut ticket cost in half

                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    if (curiosityUser != null)
                    {
                        if (curiosityUser.Job != ePlayerJobs.POLICE_OFFICER)
                            return false;

                        int userCharacterId = curiosityUser.Character.CharacterId;
                        await Database.Store.SkillDatabase.Adjust(userCharacterId, DB_POLICE_SKILL, 250);
                        await Database.Store.StatDatabase.Adjust(userCharacterId, Stat.POLICE_REPUATATION, 100);
                        await Database.Store.BankDatabase.Adjust(userCharacterId, 2500);
                        discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}' jailed by '{curiosityUser.LatestName}'");
                        SendNotification(message: $"{player.Name} has been jailed by {curiosityUser.LatestName}");

                        Vector3 position = officer.Character.Position;
                        List<int> police = GetPlayersWhoArePolice();
                        // List<Player> players = Instance.GetPlayersInRange(position, 50f);

                        foreach(int serverHandle in police)
                        {
                            Player playerToReward = PluginManager.GetPlayer(serverHandle);
                            if (playerToReward is null) continue;
                            if (Vector3.Distance(position, playerToReward.Character.Position) > 20f) continue;
                            
                            if (!PluginManager.ActiveUsers.ContainsKey(serverHandle)) continue;
                            CuriosityUser cUser = PluginManager.ActiveUsers[serverHandle];

                            int characterId = cUser.Character.CharacterId;
                            await Database.Store.SkillDatabase.Adjust(characterId, DB_POLICE_SKILL, 125);
                            await Database.Store.StatDatabase.Adjust(userCharacterId, Stat.POLICE_REPUATATION, 50);
                            await Database.Store.BankDatabase.Adjust(characterId, 1250);
                            SendNotification(serverHandle, message: $"You have been awarded for assisting in an arrest.", notification: eNotification.NOTIFICATION_SUCCESS);
                        }

                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.Attach("police:suspect:jail:self", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    Player player = PluginManager.PlayersList[metadata.Sender];
                    if (player == null)
                    {
                        Logger.Debug($"police:suspect:jail:self; Player not found");
                        return false;
                    }

                    bool isPlayerWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                    if (!isPlayerWanted)
                    {
                        Logger.Debug($"police:suspect:jail:self; Player is not wanted");
                        return false;
                    }

                    bool isPlayerJailed = player.State.Get(StateBagKey.IS_JAILED) ?? false;
                    if (isPlayerJailed)
                    {
                        Logger.Debug($"police:suspect:jail:self; Player is currently jailed?!");
                        return false;
                    }

                    await SendSuspectToJail(metadata.Sender, player, true);
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}' has jailed themselves.");
                    SendNotification(message: $"{player.Name} has jailed themselves.");

                    await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, 500 * -1);
                    curiosityUser.NotificationSuccess($"You have been charged ${500:N0} for handing yourself in.");

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.Attach("police:suspect:jail:combatLogged", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    Player player = PluginManager.PlayersList[metadata.Sender];
                    if (player == null)
                    {
                        Logger.Debug($"police:suspect:jail:log; Player not found");
                        return false;
                    }

                    bool isPlayerJailed = player.State.Get(StateBagKey.IS_JAILED) ?? false;
                    if (isPlayerJailed)
                    {
                        Logger.Debug($"police:suspect:jail:log; Player is currently jailed?!");
                        return false;
                    }

                    await SendSuspectToJail(metadata.Sender, player, combatLogged: true);
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}' has reconnected and has been jailed.");
                    await BaseScript.Delay(0);
                    SendNotification(message: $"{player.Name} has reconnected and has been jailed.");
                    await BaseScript.Delay(0);
                    curiosityUser.NotificationWarning($"Please don't combat or jail log in the future.", "bottom-left");
                    await BaseScript.Delay(0);
                    curiosityUser.NotificationWarning($"Please don't combat or jail log in the future.", "bottom");
                    await BaseScript.Delay(0);
                    curiosityUser.NotificationWarning($"Please don't combat or jail log in the future.", "bottom-right");

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.Attach("police:player:jail:served", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.Character.IsWanted = false;
                Player player = PluginManager.PlayersList[metadata.Sender];
                player.State.Set(StateBagKey.IS_JAILED, false, true);
                player.State.Set(StateBagKey.PLAYER_POLICE_WANTED, false, true);
                return false;
            }));

            EventSystem.Attach("police:playerTazedPlayer", new AsyncEventCallback(async metadata =>
            {
                int attackerServerId = metadata.Find<int>(0);
                int victimServerId = metadata.Find<int>(1);

                if (attackerServerId == victimServerId) return null;

                Player attacker = PluginManager.PlayersList[attackerServerId];
                Player victim = PluginManager.PlayersList[victimServerId];

                bool isVictimPassive = victim.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                bool isAttackerPassive = attacker.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;

                bool victimIsWanted = victim.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                bool attackerIsOfficer = (attacker.State.Get(StateBagKey.PLAYER_JOB) ?? 0) == (int)ePlayerJobs.POLICE_OFFICER;

                if (isAttackerPassive)
                {
                    SendNotification(attackerServerId, $"You have attacked someone while in a passive state, go straight to Jail. No pass go, do not pick up $200.");
                    await SendSuspectToJail(attackerServerId, attacker);

                    return null;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[int.Parse(attacker.Handle)];
                curiosityUser.Character.IsWanted = true;

                attacker.State.Set(StateBagKey.PLAYER_POLICE_WANTED, true, true);
                SendNotification(attackerServerId, $"You are now wanted.");

                Dictionary<string, string> tableRows = new Dictionary<string, string>();
                tableRows.Add("Player", attacker.Name);
                tableRows.Add("Info", "Wanted by Police");

                string notificationTable = CreateBasicNotificationTable("Player abusing stungun!", tableRows);
                SendNotification(SEND_JOB_ONLY, notificationTable);

                return null;
            }));

            EventSystem.Attach("police:officerTazedPlayer", new AsyncEventCallback(async metadata =>
            {
                int attackerServerId = metadata.Find<int>(0);
                int victimServerId = metadata.Find<int>(1);

                if (attackerServerId == victimServerId) return null;

                Player attacker = PluginManager.PlayersList[attackerServerId];
                Player victim = PluginManager.PlayersList[victimServerId];

                bool isVictimPassive = victim.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                bool isAttackerPassive = attacker.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;

                bool victimIsWanted = victim.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                bool attackerIsOfficer = (attacker.State.Get(StateBagKey.PLAYER_JOB) ?? 0) == (int)ePlayerJobs.POLICE_OFFICER;

                if (isAttackerPassive)
                {
                    SendNotification(attackerServerId, $"You have killed someone while in a passive state, go straight to Jail. No pass go, do not pick up $200.");
                    await SendSuspectToJail(attackerServerId, attacker);

                    return null;
                }

                if (attackerIsOfficer)
                {
                    if (!victimIsWanted)
                    {
                        attacker.State.Set(StateBagKey.PLAYER_POLICE_WANTED, true, true);

                        CuriosityUser curiosityUser = PluginManager.ActiveUsers[attackerServerId];
                        curiosityUser.Job = ePlayerJobs.UNEMPLOYED;
                        curiosityUser.JobCallSign = string.Empty;
                        curiosityUser.Character.IsWanted = true;
                        attacker.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUser.Job, true);

                        SendNotification(attackerServerId, $"You have removed from the force for tazing an innocent player, and are now wanted.");

                        Dictionary<string, string> tableRows = new Dictionary<string, string>();
                        tableRows.Add("Officer", attacker.Name);
                        tableRows.Add("Info", "Wanted by Police");

                        string notificationTable = CreateBasicNotificationTable("Innocent Attacked by Officer!", tableRows);
                        SendNotification(SEND_JOB_ONLY, notificationTable);
                    }
                }

                return null;
            }));

            EventSystem.Attach("police:playerKilledPlayer", new AsyncEventCallback(async metadata =>
            {
                int attackerServerId = metadata.Find<int>(0);
                int victimServerId = metadata.Find<int>(1);
                bool isMeleeDamage = metadata.Find<bool>(2);
                uint weaponInfoHash = metadata.Find<uint>(3);
                int damageTypeFlag = metadata.Find<int>(4);
                int numberOfSurroundingPeds = metadata.Find<int>(5);

                if (attackerServerId == victimServerId) return null;

                Player attacker = PluginManager.PlayersList[attackerServerId];
                Player victim = PluginManager.PlayersList[victimServerId];

                CuriosityUser curiosityUserAttacker = PluginManager.ActiveUsers[attackerServerId];
                CuriosityUser curiosityUserVictim = PluginManager.ActiveUsers[victimServerId];

                bool isAttackerWanted = attacker.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

                bool isVictimPassive = victim.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                bool isAttackerPassive = attacker.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;

                bool victimIsWanted = victim.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                bool attackerIsOfficer = (attacker.State.Get(StateBagKey.PLAYER_JOB) ?? 0) == (int)ePlayerJobs.POLICE_OFFICER;
                bool victimIsOfficer = (victim.State.Get(StateBagKey.PLAYER_JOB) ?? 0) == (int)ePlayerJobs.POLICE_OFFICER;

                if (isAttackerPassive)
                {
                    SendNotification(attackerServerId, $"You have killed someone while in a passive state, go straight to Jail. No pass go, do not pick up $200.");
                    await BaseScript.Delay(0);
                    await SendSuspectToJail(attackerServerId, attacker);

                    return null;
                }

                if (attackerIsOfficer)
                {
                    if (victimIsWanted)
                    {
                        SetEntityDistanceCullingRadius(victim.Character.Handle, 0f); // reset culling
                        victim.State.Set(StateBagKey.PLAYER_POLICE_WANTED, false, true); // cannot want a dead person
                        victim.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 0, true);
                        curiosityUserVictim.Character.IsWanted = false;
                    }

                    if (!victimIsWanted)
                    {
                        attacker.State.Set(StateBagKey.PLAYER_POLICE_WANTED, true, true);
                        attacker.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 10, true);
                        curiosityUserAttacker.Character.IsWanted = true;

                        curiosityUserAttacker.Job = ePlayerJobs.UNEMPLOYED;
                        curiosityUserAttacker.JobCallSign = string.Empty;
                        attacker.State.Set(StateBagKey.PLAYER_JOB, (int)curiosityUserAttacker.Job, true);

                        SendNotification(attackerServerId, $"You have removed from the force for killing an innocent.");

                        Dictionary<string, string> tableRows = new Dictionary<string, string>();
                        tableRows.Add("Officer", attacker.Name);
                        tableRows.Add("Info", "Wanted by Police");

                        SetEntityDistanceCullingRadius(attacker.Character.Handle, 15000f); // make the player visible

                        if (!playerCullingReset.ContainsKey(attacker))
                            playerCullingReset.Add(attacker, GetGameTimer() + TIME_TIL_CULLING_RESET);

                        string notificationTable = CreateBasicNotificationTable("Innocent Killed by Officer!", tableRows);
                        await BaseScript.Delay(0);
                        SendNotification(SEND_JOB_ONLY, notificationTable);
                    }
                }

                if (!attackerIsOfficer)
                {
                    if (!isAttackerWanted)
                    {
                        string weaponName = DeathHash.CauseOfDeath[(int)weaponInfoHash];

                        Dictionary<string, string> tableRows = new Dictionary<string, string>();
                        tableRows.Add("Attacker", attacker.Name);
                        tableRows.Add("Victim", victim.Name);
                        tableRows.Add("Weapon", weaponName);
                        tableRows.Add("Info", "Wanted by Police");

                        attacker.State.Set(StateBagKey.PLAYER_POLICE_WANTED, true, true);
                        attacker.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 10, true);
                        curiosityUserAttacker.Character.IsWanted = true;
                        SetEntityDistanceCullingRadius(attacker.Character.Handle, 15000f); // make the player visible

                        if (!playerCullingReset.ContainsKey(attacker))
                            playerCullingReset.Add(attacker, GetGameTimer() + TIME_TIL_CULLING_RESET);

                        string notificationTable = CreateBasicNotificationTable("Player Killed", tableRows);
                        await BaseScript.Delay(0);
                        SendNotification(SEND_JOB_ONLY, notificationTable);
                    }

                    if (victimIsOfficer)
                    {
                        int characterId = curiosityUserAttacker.Character.CharacterId;
                        Database.Store.SkillDatabase.Adjust(characterId, Skill.CRIMINAL, 25);
                        curiosityUserAttacker.NotificationSuccess("Rewarded 25xp");
                    }
                }
                // log kill & reward officers (if kill is near a safe area, its not rewarded)

                return null;
            }));

            EventSystem.Attach("police:report:murder", new AsyncEventCallback(async metadata =>
            {
                return null;
            }));

            EventSystem.Attach("police:report:notification:toggle", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.DisableNotifications = !curiosityUser.DisableNotifications;
                return curiosityUser.DisableNotifications;
            }));

            EventSystem.Attach("police:player:isJailed", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                return false;
            }));

            EventSystem.Attach("police:ticket:speeding", new AsyncEventCallback(async metadata =>
            {
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
                    int vehicleNetId = metadata.Find<int>(2);
                    string street = metadata.Find<string>(3);
                    string direction = metadata.Find<string>(4);
                    bool isWrongWay = metadata.Find<bool>(5);

                    Logger.Debug($"Speeding Reported: {speed} / {speedLimit} / {vehicleNetId} / {street} / {direction} / {isWrongWay}");

                    // get Vehicle
                    int vehicleHandle = NetworkGetEntityFromNetworkId(vehicleNetId);
                    Vehicle vehicle = new Vehicle(vehicleHandle);

                    bool informPolice = (speed - speedLimit) > serverConfigManager.PoliceSpeedLimitWarning;

                    if (vehicle is null)
                    {
                        Logger.Debug($"Vehicle no found");
                        em.error = "Vehicle not found.";
                        goto RETURN_MESSAGE;
                    }

                    // add ticket to the database against the character/vehicle
                    int characterId = curiosityUser.Character.CharacterId;
                    int characterVehicleId = vehicle.State.Get(StateBagKey.VEH_ID); // mark in the Database

                    if (characterVehicleId == 0)
                    {
                        Logger.Error($"Issue saving ticket, vehicle is unknown.");
                        em.error = "Vehicle is unknown to the server.";
                        goto RETURN_MESSAGE;
                    }

                    int costOfTicket = (int)((speed - speedLimit) * (isWrongWay ? 75 : 50)); // only charge for speed over the limit

                    // TODO: Move the DateTime into the database
                    bool success = await Database.Store.PoliceDatabase.InsertTicket(ePoliceTicketType.SPEEDING, characterId, characterVehicleId, costOfTicket, DateTime.UtcNow.AddDays(7), speed, speedLimit);

                    if (!success)
                    {
                        Logger.Error($"Issue saving ticket");
                        em.error = "Issue when trying to save ticket.";
                        goto RETURN_MESSAGE;
                    }

                    string numberPlate = GetVehicleNumberPlateText(vehicle.Handle);

                    string playerMsg = $"<table width=\"300\"><thead><tr><th colspan=\"2\">Caught Speeding ({costOfTicket:C0})</th></tr></thead>" +
                        $"<tbody><tr><td scope=\"row\" width=\"236\">" +
                        $"Location: {street}<br />Heading: {direction}<br />License Plate: {numberPlate}<br />Owner: {player.Name}<br />Speed: {speed} MPH";
                    playerMsg += $"</td><td><img src=\"./assets/img/icons/speedCameraWhite.png\" width=\"64\" /></td></tr></tbody></table>";

                    SendNotification(metadata.Sender, playerMsg, vehicleNetId: vehicle.NetworkId);

                    bool isPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? curiosityUser.Character.IsPassive;
                    bool isOfficer = curiosityUser.Job == ePlayerJobs.POLICE_OFFICER;

                    if (informPolice && !isPassive && !isOfficer)
                    {
                        bool isWreckless = (speed - speedLimit) > (serverConfigManager.PoliceSpeedLimitWarning + 20);
                        // wanted flag so police are not punished

                        player.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 1, true);
                        bool isWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

                        if (isWreckless)
                        {
                            player.State.Set(StateBagKey.PLAYER_POLICE_WANTED, true, true);
                            curiosityUser.Character.IsWanted = true;
                        }

                        SetEntityDistanceCullingRadius(player.Character.Handle, 15000f); // make the player visible

                        if (!playerCullingReset.ContainsKey(player))
                            playerCullingReset.Add(player, GetGameTimer() + TIME_TIL_CULLING_RESET);

                        string msg = $"<table width=\"300\"><thead><tr><th colspan=\"2\">Speeding Report</th></tr></thead>" +
                        $"<tbody><tr><td scope=\"row\" width=\"236\">" +
                        $"Last Location: {street}<br />Heading: {direction}<br />License Plate: {numberPlate}<br />Owner: {player.Name}<br />Speed: {speed} MPH" +
                        $"</td><td><img src=\"./assets/img/icons/speedCameraWhite.png\" width=\"64\" /></td></tr></tbody></table>";

                        SendNotification(serverId: SEND_JOB_ONLY, message: msg, vehicleNetId: vehicle.NetworkId);
                        Logger.Debug($"Speeding police reported");

                    }

                    Logger.Debug($"Speeding: {speed} / {speedLimit} / {vehicleNetId} / {street} / {direction}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"police:ticket:speeding");
                    em.error = $"Error when invoicing speeding ticket.";
                }

            RETURN_MESSAGE:
                return em;
            }));

            EventSystem.Attach("mission:assistance:list", new EventCallback(metadata =>
            {
                long gameTimer = GetGameTimer();
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                List<GenericUserListItem> curiosityUsersRequesting = new();
                List<CuriosityUser> users = GetUsersWhoArePolice();

                foreach (CuriosityUser user in users)
                {
                    if (!user.AssistanceRequested) continue;
                    if ((gameTimer - user.LastNotificationBackup) > TWO_MINUTES) continue;

                    GenericUserListItem genericUserListItem = new();
                    genericUserListItem.Name = user.LatestName;
                    genericUserListItem.ServerId = user.Handle;
                    curiosityUsersRequesting.Add(genericUserListItem);
                }

                Logger.Debug($"Returning {curiosityUsersRequesting.Count} for mission:assistance:list");
                return curiosityUsersRequesting;
            }));
        }

        private async Task SendSuspectToJail(int suspectServerId, Player player, bool jailSelf = false, bool combatLogged = false)
        {
            SetEntityDistanceCullingRadius(player.Character.Handle, 0f); // reset culling
            player.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 0, true);
            player.State.Set(StateBagKey.PLAYER_POLICE_WANTED, false, true);
            player.State.Set(StateBagKey.IS_JAILED, true, true);

            EventSystem.Send("police:suspect:jail", suspectServerId, jailSelf, combatLogged); // jail
            CuriosityUser curiosityUserSuspect = PluginManager.ActiveUsers[suspectServerId];

            int staffVehicle = curiosityUserSuspect.StaffVehicle;
            int playerVehicle = curiosityUserSuspect.PersonalVehicle;
            int playerBoat = curiosityUserSuspect.PersonalBoat;
            int playerTrailer = curiosityUserSuspect.PersonalTrailer;
            int playerPlane = curiosityUserSuspect.PersonalPlane;
            int playerHelicopter = curiosityUserSuspect.PersonalHelicopter;

            if (staffVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(staffVehicle);
            await BaseScript.Delay(100);
            if (playerVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerVehicle);
            await BaseScript.Delay(100);
            if (playerBoat > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerBoat);
            await BaseScript.Delay(100);
            if (playerTrailer > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerTrailer);
            await BaseScript.Delay(100);
            if (playerPlane > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerPlane);
            await BaseScript.Delay(100);
            if (playerHelicopter > 0) EntityManager.EntityInstance.NetworkDeleteEntity(playerHelicopter);
            await BaseScript.Delay(100);
        }

        void SendNotification(int serverId = -1, string message = "", eNotification notification = eNotification.NOTIFICATION_INFO, int duration = 5000, int vehicleNetId = -1)
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
                .Where(y => y.Value.Job == ePlayerJobs.POLICE_OFFICER && !y.Value.DisableNotifications)
                .Select(x => x.Key).ToList();
        }

        List<CuriosityUser> GetUsersWhoArePolice()
        {
            return PluginManager.ActiveUsers
                .Where(y => y.Value.Job == ePlayerJobs.POLICE_OFFICER)
                .Select(x => x.Value).ToList();
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

        public async Task<bool> SetUserJobText(int playerServerId, string jobText)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(playerServerId)) return false;

            CuriosityUser curiosityUser = PluginManager.ActiveUsers[playerServerId];

            switch (jobText)
            {
                case "Police Officer":
                case "Police Officer [PvP]":
                    string concatJob = string.Concat(jobText.Where(c => char.IsUpper(c)));
                    string randomStr = await CreateUniqueCallSign();
                    curiosityUser.JobCallSign = $"{concatJob}-{randomStr}";
                    break;
                default:
                    curiosityUser.JobCallSign = string.Empty;
                    break;
            }

            curiosityUser.CurrentJob = jobText;

            return true;
        }

        async Task<string> CreateUniqueCallSign()
        {
            List<string> currentCallSigns = new List<string>();

            foreach (KeyValuePair<int, CuriosityUser> u in PluginManager.ActiveUsers)
            {
                currentCallSigns.Add(u.Value.JobCallSign);
            }

            string callsign = GenerateRandomAlphanumericString(CALL_SIGN_LENGTH);

            //while (true)
            //{
            //    await BaseScript.Delay(0);

            //    if (currentCallSigns.Count == 0)
            //        break;

            //    if (!currentCallSigns.Contains(callsign))
            //        break;

            //    callsign = GenerateRandomAlphanumericString(CALL_SIGN_LENGTH);
            //}

            return callsign;
        }

        string GenerateRandomAlphanumericString(int length = 10)
        {
            // const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const string chars = "0123456789";

            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[Utility.RANDOM.Next(s.Length)]).ToArray());
            return randomString;
        }
    }
}
