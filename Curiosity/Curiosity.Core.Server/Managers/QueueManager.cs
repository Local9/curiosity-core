using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class QueueManager : Manager<QueueManager>
    {
        static Dictionary<Messages, string> messages = new Dictionary<Messages, string>();

        static Regex regex = new Regex(@"^[ :A-Za-z0-9-_.#\[\]]{1,32}$");
        static Regex blacklistedNames = new Regex(@"\b(admin|nigga|nigger|administrator|moderator|staff|n1gg3|n1g|n1gg3r|user|pc)\b");

        static string resourceName = API.GetCurrentResourceName();
        static string resourcePath = $"resources/{API.GetResourcePath(resourceName).Substring(API.GetResourcePath(resourceName).LastIndexOf("//") + 2)}";

        static string hostName = string.Empty;
        static bool stateChangeMessages = false;
        // Internal Queue Settings
        static int inQueue = 0;
        static int inPriorityQueue = 0;
        static int lastCount = 0;
        // Configurable Queue Settings
        static int queueGraceTime = 2;
        static int graceTime = 2;
        static int loadTime = 10;
        static int reservedSlots = 2;
        static int publicTypeSlots = 0;
        static int maxSession = 32;

        bool queueReadoutEnabled = false;

        static bool IsServerQueueReady = false;
        static DateTime serverStartTime = DateTime.Now;

        // Concurrent Values
        public static ConcurrentDictionary<string, SessionState> session = new ConcurrentDictionary<string, SessionState>();

        static ConcurrentDictionary<string, int> priority = new ConcurrentDictionary<string, int>();

        static ConcurrentDictionary<string, int> index = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<string, DateTime> timer = new ConcurrentDictionary<string, DateTime>();
        static ConcurrentDictionary<string, Player> sentLoading = new ConcurrentDictionary<string, Player>();
        // Slots
        static ConcurrentDictionary<string, Reserved> reserved = new ConcurrentDictionary<string, Reserved>();
        static ConcurrentDictionary<string, Reserved> slotTaken = new ConcurrentDictionary<string, Reserved>();
        // Concurrent Queues:
        static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        static ConcurrentQueue<string> newQueue = new ConcurrentQueue<string>();

        static ConcurrentQueue<string> priorityQueue = new ConcurrentQueue<string>();
        static ConcurrentQueue<string> newPriorityQueue = new ConcurrentQueue<string>();

        public override void Begin()
        {
            Logger.Debug($"[QueueManager] Begin");

            SetupConvars();

            Instance.EventRegistry["playerConnecting"] += new Action<Player, string, CallbackDelegate, ExpandoObject>(OnConnect);
            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            Instance.EventRegistry["onResourceStop"] += new Action<string>(OnResourceStop);

            Instance.AttachTickHandler(SetupTimer);

            EventSystem.GetModule().Attach("user:queue:active", new EventCallback(metadata =>
            {
                try
                {
                    Player player = PluginManager.PlayersList[metadata.Sender];

                    string license = player.Identifiers["license"];
                    if (!session.ContainsKey(license))
                    {
                        session.TryAdd(license, SessionState.Active);
                        return false;
                    }
                    session.TryGetValue(license, out SessionState oldState);
                    session.TryUpdate(license, SessionState.Active, oldState);
                    if (stateChangeMessages) { Logger.Debug($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> ACTIVE -> {license}"); }

                    // Custom Changes
                    string msg = $"Player '{player.Name}#{metadata.Sender}': Successfully Connected. Ping: {player.Ping}ms";
                    DiscordClient.GetModule().SendDiscordPlayerLogMessage(msg);
                    ChatManager.OnLogMessage(msg);

                    int initRouting = 5000;
                    int playerHandle = int.Parse(player.Handle);
                    int routingId = initRouting + playerHandle;

                    API.SetPlayerRoutingBucket(player.Handle, routingId);

                    player.State.Set($"{StateBagKey.PLAYER_ROUTING}", routingId, true);
                    player.State.Set($"{StateBagKey.PLAYER_NAME}", player.Name, true);
                    player.State.Set($"{StateBagKey.SERVER_HANDLE}", player.Handle, true);
                    player.State.Set($"{StateBagKey.PLAYER_MENU}", false, true);

                    //if (PluginManager.IsDebugging)
                    //    DiscordClient.GetModule().SendDiscordServerEventLogMessage($"Queue: {queue.Count}, Sessions: {session.Count}, Active Players: {PluginManager.PlayersList.Count()}, User Cache: {PluginManager.ActiveUsers.Count}");

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Curiosity Queue Manager : user:queue:active -> {ex}");
                    return false;
                }
            }));

            Logger.Debug($"[QueueManager] End");
        }

        private async void OnConnect([FromSource] Player player, string name, CallbackDelegate denyWithReason, dynamic deferrals)
        {
            try
            {
                int initRouting = 5000;
                int playerHandle = int.Parse(player.Handle);
                int routingId = initRouting + playerHandle;

                API.SetPlayerRoutingBucket(player.Handle, routingId);

                player.State.Set($"{StateBagKey.PLAYER_ROUTING}", routingId, true);
                player.State.Set($"{StateBagKey.PLAYER_NAME}", player.Name, true);
                player.State.Set($"{StateBagKey.SERVER_HANDLE}", player.Handle, true);

                string license = player.Identifiers["license"];

                while (!PluginManager.ServerReady)
                {
                    await BaseScript.Delay(500);
                    deferrals.update("Awaiting Server Startup.");
                    await BaseScript.Delay(500);
                    deferrals.update("Awaiting Server Startup..");
                    await BaseScript.Delay(500);
                    deferrals.update("Awaiting Server Startup...");
                }

                string msg = $"Player '{player.Name}' is connecting. Ping: {player.Ping}ms";
                DiscordClient discordClient = DiscordClient.GetModule();
                discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}' is connecting. Ping: {player.Ping}ms");
                ChatManager.OnLogMessage(msg);

                deferrals.update($"{messages[Messages.Gathering]}");

                if (string.IsNullOrEmpty(license)) // No License, No Gameplay
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': No License, No Game.");
                    deferrals.done($"{messages[Messages.License]}");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                if (string.IsNullOrEmpty(player.Name))
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player name could not be found.");
                    deferrals.done($"{string.Format(messages[Messages.NoName], player.Name)}");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                if (!regex.IsMatch(player.Name))
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': Name contains symbols.");
                    deferrals.done($"{string.Format(messages[Messages.Symbols], player.Name)}");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                if (blacklistedNames.IsMatch(player.Name.ToLower()))
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': Name is blacklisted.");
                    deferrals.done($"The username of '{player.Name}' is blacklisted, please change your username and try to rejoin.");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                bool isVerified = await discordClient.CheckDiscordIdIsInGuild(player);

                if (!isVerified)
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': Not verified on Discord.");
                    deferrals.done($"Unabled to verify Discord Authorisation.\n\nJoin {PluginManager.DiscordUrl} and accept the verification process.");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                await BaseScript.Delay(10);

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

                await BaseScript.Delay(10);

                if (curiosityUser == null)
                {
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': Error loading Account.");
                    deferrals.done($"Sorry, there was an error when trying to load your account.");
                    RemoveFrom(license, true, true, true, true, true, true);
                    return;
                }

                if (curiosityUser.IsBanned)
                {
                    string banMessage = "Your user account is currently banned.";
                    try
                    {
                        DateTime date = (DateTime)curiosityUser.BannedUntil;
                        string dateStr = date.ToString("yyyy-MM-dd HH:mm");
                        string time = $"Until {dateStr}";
                        if (curiosityUser.IsBannedPerm)
                            time = "Permanently";

                        discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}#{curiosityUser.UserId}': Is Banned - {time}.");
                        deferrals.done(string.Format("{0} {1}", banMessage, time));
                    }
                    catch (Exception ex)
                    {
                        deferrals.done($"{banMessage}");
                    }
                    finally
                    {
                        RemoveFrom(license, true, true, true, true, true, true);
                    }
                    return;
                }

                if (!curiosityUser.IsStaff)
                {
                    if (PluginManager.IsMaintenanceActive)
                    {
                        deferrals.done($"Curiosity Queue Manager : This server is in a testing state, please make sure you are connecting to the correct server or use the links provided at {PluginManager.WebsiteUrl}.");
                        RemoveFrom(license, true, true, true, true, true, true);
                        return;
                    }
                }

                if (PluginManager.IsSupporterAccess && !curiosityUser.IsSupporterAccess)
                {
                    Logger.Debug($"Queue Player not allowed access: {player.Name}#{curiosityUser.UserId} ({curiosityUser.Role}) [U:{curiosityUser.IsSupporterAccess}/S:{PluginManager.IsSupporterAccess}]");
                    discordClient.SendDiscordPlayerLogMessage($"Player '{player.Name}': user is not a supporter, current role '{curiosityUser.Role}' [U:{curiosityUser.IsSupporterAccess}/S:{PluginManager.IsSupporterAccess}].");
                    deferrals.done($"Server is only allowing connections from supporters.\n\nDiscord URL: discord.lifev.net");
                    return;
                }

                if (sentLoading.ContainsKey(license))
                {
                    sentLoading.TryRemove(license, out Player oldPlayer);
                }
                sentLoading.TryAdd(license, player);

                if (curiosityUser.QueuePriority > 0 || curiosityUser.IsStaff)
                {
                    if (curiosityUser.IsStaff)
                    {
                        Logger.Debug($"Curiosity Queue Manager : Staff Member {curiosityUser.LatestName} added to Priority Queue");
                    }

                    if (!priority.TryAdd(license, curiosityUser.QueuePriority))
                    {
                        priority.TryGetValue(license, out int oldPriority);
                        priority.TryUpdate(license, curiosityUser.QueuePriority, oldPriority);
                    }
                }

                if (session.TryAdd(license, SessionState.Queue))
                {
                    if (!priority.ContainsKey(license))
                    {
                        newQueue.Enqueue(license);
                        if (stateChangeMessages) { Logger.Debug($"Curiosity Queue Manager : NEW -> QUEUE -> (Public) {player.Name} [{license}]"); }
                    }
                    else
                    {
                        newPriorityQueue.Enqueue(license);
                        if (stateChangeMessages) { Logger.Debug($"Curiosity Queue Manager : NEW -> QUEUE -> (Priority) {player.Name} [{license}]"); }
                    }
                }

                if (!session[license].Equals(SessionState.Queue))
                {
                    UpdateTimer(license);
                    session.TryGetValue(license, out SessionState oldState);
                    session.TryUpdate(license, SessionState.Loading, oldState);
                    deferrals.done();
                    if (stateChangeMessages) { Logger.Debug($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> LOADING -> (Grace) {player.Name} [{license}]"); }
                    return;
                }

                bool inPriority = priority.ContainsKey(license);
                int dots = 0;

                while (session[license].Equals(SessionState.Queue))
                {
                    if (index.ContainsKey(license) && index.TryGetValue(license, out int position))
                    {
                        int count = inPriority ? inPriorityQueue : inQueue;
                        string message = inPriority ? $"{messages[Messages.PriorityQueue]}" : $"{messages[Messages.Queue]}";
                        deferrals.update($"{message} {position} / {count}{new string('.', dots)}");
                    }
                    dots = dots > 2 ? 0 : dots + 1;
                    if (player?.EndPoint is null)
                    {
                        UpdateTimer(license);
                        deferrals.done($"{messages[Messages.Canceled]}");
                        if (stateChangeMessages) { Logger.Debug($"Curiosity Queue Manager : QUEUE -> CANCELED -> {license}"); }
                        return;
                    }
                    RemoveFrom(license, false, false, true, false, false, false);
                    await BaseScript.Delay(5000);
                }
                await BaseScript.Delay(500);

                deferrals.done();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Queue Error");
                deferrals.done($"Error in Queue");
                return;
            }
        }

        private async Task QueueUpdate()
        {
            if (!queueReadoutEnabled)
                Instance.DetachTickHandler(QueueUpdate);

            if (inPriorityQueue > 0 || inQueue > 0 || session.Count > 0)
            {
                int activeSessions = session.Where(x => x.Value == SessionState.Active).Count();
                int graceSessions = session.Where(x => x.Value == SessionState.Grace).Count();
                int loadingSessions = session.Where(x => x.Value == SessionState.Loading).Count();
                int queuedSessions = session.Where(x => x.Value == SessionState.Queue).Count();

                string msg = $"Queue Update";
                msg += $"\n - inPriorityQueue: {inPriorityQueue}";
                msg += $"\n - inQueue: {inQueue}";
                msg += $"\n -----";
                msg += $"\n Sessions: {session.Count}/{maxSession}";
                msg += $"\n Active: {activeSessions}";
                msg += $"\n Graced: {graceSessions}";
                msg += $"\n Loading: {loadingSessions}";
                msg += $"\n Queued: {queuedSessions}";
                DiscordClient.GetModule().SendDiscordServerEventLogMessage(msg);
            }
            int timeToWait = 60 * 1000;
            await BaseScript.Delay(timeToWait);
        }

        async Task SetupTimer()
        {
            if (PluginManager.IsMaintenanceActive)
            {
                IsServerQueueReady = true;
                Instance.DetachTickHandler(SetupTimer);
                Logger.Debug("[MAINTENANCE] Server Queue is ready.");
                return;
            }

            if (DateTime.Now.Subtract(serverStartTime).TotalSeconds > 30)
            {
                IsServerQueueReady = true;
                Instance.DetachTickHandler(SetupTimer);
                Logger.Debug("Server Queue is ready to accept players.");
            }
            await Task.FromResult(1000);
        }

        [TickHandler]
        private async Task QueueCycle()
        {
            while (true)
            {
                try
                {
                    inPriorityQueue = PriorityQueueCount();
                    await BaseScript.Delay(100);
                    inQueue = QueueCount();
                    await BaseScript.Delay(100);
                    UpdateHostName();
                    UpdateStates();
                    await BaseScript.Delay(100);
                    BalanceReserved();
                    await BaseScript.Delay(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Curiosity Queue Manager : QueueCycle() -> {ex.Message}");
                }
            }
        }

        async void StopHardcap()
        {
            try
            {
                API.ExecuteCommand($"sets fivemqueue Enabled");
                int attempts = 0;
                while (attempts < 7)
                {
                    attempts += 1;
                    string state = API.GetResourceState("hardcap");
                    if (state == "missing")
                    {
                        break;
                    }
                    else if (state == "started")
                    {
                        API.StopResource("hardcap");
                        break;
                    }
                    await BaseScript.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : StopHardcap()");
            }
        }


        void OnResourceStop(string name)
        {
            try
            {
                if (name == resourceName)
                {
                    if (API.GetResourceState("hardcap") != "started")
                    {
                        API.StartResource("hardcap");
                        API.ExecuteCommand($"sets fivemqueue Disabled");
                    }
                    if (hostName != string.Empty) { API.SetConvar("sv_hostname", hostName); return; }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : OnResourceStop()");
            }
        }

        void UpdateStates()
        {
            try
            {
                session.Where(k => k.Value == SessionState.Loading || k.Value == SessionState.Grace).ToList().ForEach(j =>
                {
                    try
                    {
                        string license = j.Key;
                        SessionState state = j.Value;
                        PlayerList players = PluginManager.PlayersList;
                        switch (state)
                        {
                            case SessionState.Loading:
                                if (!timer.TryGetValue(license, out DateTime oldLoadTime))
                                {
                                    UpdateTimer(license);
                                    break;
                                }
                                if (IsTimeUp(license, loadTime))
                                {
                                    if (players.FirstOrDefault(i => i.Identifiers["license"] == license)?.EndPoint != null)
                                    {
                                        players.FirstOrDefault(i => i.Identifiers["license"] == license).Drop($"{messages[Messages.Timeout]}");
                                    }
                                    session.TryGetValue(license, out SessionState oldState);
                                    session.TryUpdate(license, SessionState.Grace, oldState);
                                    UpdateTimer(license);
                                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : LOADING -> GRACE -> {license}"); }
                                }
                                break;
                            case SessionState.Grace:
                                if (!timer.TryGetValue(license, out DateTime oldGraceTime))
                                {
                                    UpdateTimer(license);
                                    break;
                                }
                                if (IsTimeUp(license, graceTime))
                                {
                                    if (players.FirstOrDefault(i => i.Identifiers["license"] == license)?.EndPoint != null)
                                    {
                                        if (!session.TryAdd(license, SessionState.Active))
                                        {
                                            session.TryGetValue(license, out SessionState oldState);
                                            session.TryUpdate(license, SessionState.Active, oldState);
                                        }
                                    }
                                    else
                                    {
                                        RemoveFrom(license, true, true, true, true, true, true);
                                        if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : GRACE -> REMOVED -> {license}"); }
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Curiosity Queue Manager : UpdateStates() : FOREACH");
                        Logger.Error($"{ex.Message}");
                        Logger.Debug($"{ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : UpdateStates()");
                Logger.Error($"{ex.Message}");
                Logger.Debug($"{ex}");
            }
        }

        static void BalanceReserved()
        {
            try
            {
                var query = from license in session
                            join license2 in reserved on license.Key equals license2.Key
                            join license3 in slotTaken on license.Key equals license3.Key
                            where license.Value == SessionState.Active && license2.Value != license3.Value
                            select new { license.Key, license2.Value };

                query.ToList().ForEach(k =>
                {
                    int openReservedTypeOneSlots = reservedSlots - slotTaken.Count(j => j.Value == Reserved.Reserved);

                    switch (k.Value)
                    {
                        case Reserved.Reserved:
                            if (openReservedTypeOneSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved1"); }
                            }
                            break;
                        default:
                            if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Public"); }
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : BalanceReserved()");
            }
        }

        void UpdateHostName()
        {
            try
            {
                if (hostName == string.Empty) { hostName = API.GetConvar("sv_hostname", string.Empty); }
                if (hostName == string.Empty) { return; }

                string concat = hostName;
                bool editHost = false;
                int count = inQueue + inPriorityQueue;
                if (API.GetConvar("queue_add_count_before_name", "false") == "true")
                {
                    editHost = true;
                    if (count > 0) { concat = string.Format($"{messages[Messages.QueueCount]} {concat}", count); }
                    else { concat = hostName; }
                }
                if (API.GetConvar("queue_add_count_after_name", "false") == "true")
                {
                    editHost = true;
                    if (count > 0) { concat = string.Format($"{concat} {messages[Messages.QueueCount]}", count); }
                    else { concat = hostName; }
                }
                if (lastCount != count && editHost)
                {
                    API.SetConvar("sv_hostname", concat);
                }
                lastCount = count;
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : UpdateHostName()");
            }
        }

        int QueueCount()
        {
            try
            {
                int place = 0;
                ConcurrentQueue<string> temp = new ConcurrentQueue<string>();
                while (!queue.IsEmpty)
                {
                    queue.TryDequeue(out string license);
                    if (IsTimeUp(license, queueGraceTime))
                    {
                        RemoveFrom(license, true, true, true, true, true, true);
                        if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : CANCELED -> REMOVED -> {license}"); }
                        continue;
                    }
                    if (priority.TryGetValue(license, out int priorityAdded))
                    {
                        newPriorityQueue.Enqueue(license);
                        continue;
                    }
                    if (!Loading(license))
                    {
                        place += 1;
                        UpdatePlace(license, place);
                        temp.Enqueue(license);
                    }
                }
                while (!newQueue.IsEmpty)
                {
                    newQueue.TryDequeue(out string license);
                    if (!Loading(license))
                    {
                        place += 1;
                        UpdatePlace(license, place);
                        temp.Enqueue(license);
                    }
                }
                queue = temp;
                return queue.Count;
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : QueueCount()"); return queue.Count;
            }
        }

        bool Loading(string license)
        {
            try
            {
                if (reserved.ContainsKey(license) && reserved[license] == Reserved.Reserved && slotTaken.Count(j => j.Value == Reserved.Reserved) < reservedSlots)
                { NewLoading(license, Reserved.Reserved); return true; }
                else if (session.Count(j => j.Value != SessionState.Queue) - slotTaken.Count(i => i.Value != Reserved.Public) < publicTypeSlots)
                { NewLoading(license, Reserved.Public); return true; }
                else { return false; }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : Loading()"); return false;
            }
        }

        void NewLoading(string license, Reserved slotType)
        {
            try
            {
                if (session.TryGetValue(license, out SessionState oldState))
                {
                    UpdateTimer(license);
                    RemoveFrom(license, false, true, false, false, false, false);
                    if (!slotTaken.TryAdd(license, slotType))
                    {
                        slotTaken.TryGetValue(license, out Reserved oldSlotType);
                        slotTaken.TryUpdate(license, slotType, oldSlotType);
                    }
                    session.TryUpdate(license, SessionState.Loading, oldState);
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : QUEUE -> LOADING -> ({Enum.GetName(typeof(Reserved), slotType)}) {license}"); }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : NewLoading()");
            }
        }

        bool IsTimeUp(string license, double time)
        {
            try
            {
                if (!timer.ContainsKey(license)) { return false; }
                return timer[license].AddMinutes(time) < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : IsTimeUp()"); return false;
            }
        }

        void UpdatePlace(string license, int place)
        {
            try
            {
                if (!index.TryAdd(license, place))
                {
                    index.TryGetValue(license, out int oldPlace);
                    index.TryUpdate(license, place, oldPlace);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : UpdatePlace()");
            }
        }

        void UpdateTimer(string license)
        {
            try
            {
                if (!timer.TryAdd(license, DateTime.UtcNow))
                {
                    timer.TryGetValue(license, out DateTime oldTime);
                    timer.TryUpdate(license, DateTime.UtcNow, oldTime);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : UpdateTimer()");
            }
        }

        int PriorityQueueCount()
        {
            try
            {
                List<KeyValuePair<string, int>> order = new List<KeyValuePair<string, int>>();
                while (!priorityQueue.IsEmpty)
                {
                    priorityQueue.TryDequeue(out string license);
                    if (IsTimeUp(license, queueGraceTime))
                    {
                        RemoveFrom(license, true, true, true, true, true, true);
                        if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : CANCELED -> REMOVED -> {license}"); }
                        continue;
                    }
                    if (!priority.TryGetValue(license, out int priorityNum))
                    {
                        newQueue.Enqueue(license);
                        continue;
                    }
                    order.Insert(order.FindLastIndex(k => k.Value <= priorityNum) + 1, new KeyValuePair<string, int>(license, priorityNum));
                }
                while (!newPriorityQueue.IsEmpty)
                {
                    newPriorityQueue.TryDequeue(out string license);
                    priority.TryGetValue(license, out int priorityNum);
                    order.Insert(order.FindLastIndex(k => k.Value >= priorityNum) + 1, new KeyValuePair<string, int>(license, priorityNum));
                }
                int place = 0;
                order.ForEach(k =>
                {
                    if (!Loading(k.Key))
                    {
                        place += 1;
                        UpdatePlace(k.Key, place);
                        priorityQueue.Enqueue(k.Key);
                    }
                });
                return priorityQueue.Count;
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : PriorityQueueCount()"); return priorityQueue.Count;
            }
        }

        public void OnPlayerDropped(Player source, string message)
        {
            try
            {
                string license = source.Identifiers["license"];
                if (license == null)
                {
                    return;
                }
                if (!session.ContainsKey(license) || message == "Exited")
                {
                    return;
                }
                if (message.Contains("Kick") || message.Contains("Ban"))
                {
                    RemoveFrom(license, true, true, true, true, true, true);
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : REMOVED -> {license}"); }
                }

                bool hasState = session.TryGetValue(license, out SessionState oldState);
                if (hasState && oldState != SessionState.Queue)
                {
                    session.TryUpdate(license, SessionState.Grace, oldState);
                    if (stateChangeMessages) { Debug.WriteLine($"[{resourceName}]: {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> GRACE -> {license}"); }
                    UpdateTimer(license);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : PlayerDropped()");
            }
        }

        public void RemoveFrom(string license, bool doSession, bool doIndex, bool doTimer, bool doPriority, bool doReserved, bool doSlot)
        {
            try
            {
                if (doSession) { session.TryRemove(license, out SessionState oldState); }
                if (doIndex) { index.TryRemove(license, out int oldPosition); }
                if (doTimer) { timer.TryRemove(license, out DateTime oldTime); }
                if (doPriority) { priority.TryRemove(license, out int oldPriority); }
                if (doReserved) { reserved.TryRemove(license, out Reserved oldReserved); }
                if (doSlot) { slotTaken.TryRemove(license, out Reserved oldSlot); }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : RemoveFrom()");
            }
        }

        void SetupConvars()
        {
            stateChangeMessages = API.GetConvar("queue_enable_console_messages", "true") == "true";
            maxSession = API.GetConvarInt("queue_max_session_slots", maxSession);

            loadTime = API.GetConvarInt("queue_loading_timeout", loadTime);
            graceTime = API.GetConvarInt("queue_reconnect_timeout", graceTime);
            queueGraceTime = API.GetConvarInt("queue_cancel_timeout", queueGraceTime);
            reservedSlots = API.GetConvarInt("queue_type_1_reserved_slots", reservedSlots);
            publicTypeSlots = maxSession - reservedSlots;

            Logger.Info($"Queue Settings -> queue_max_session_slots {maxSession}");
            Logger.Info($"Queue Settings -> queue_loading_timeout {loadTime} mins");
            Logger.Info($"Queue Settings -> queue_reconnect_timeout {graceTime} mins");
            Logger.Info($"Queue Settings -> queue_cancel_timeout {queueGraceTime} mins");
            Logger.Info($"Queue Settings -> queue_reserved_slots {reservedSlots}");
            Logger.Info($"Queue Settings -> Final Public Slots: {publicTypeSlots}");

            SetupMessages();

            Logger.Success($"Queue Configuration Completed");
        }

        void SetupMessages()
        {
            if (messages.Count > 0) return;

            messages.Add(Messages.Gathering, "Placing your call with the Architect...");
            messages.Add(Messages.License, "License is required");
            messages.Add(Messages.Steam, "Steam is required");
            messages.Add(Messages.Banned, "You are banned {0}");
            messages.Add(Messages.Whitelist, "You are not whitelisted");
            messages.Add(Messages.Queue, "You are in queue");
            messages.Add(Messages.PriorityQueue, "You are in priority queue");
            messages.Add(Messages.Canceled, "Canceled from queue");
            messages.Add(Messages.Error, "An error prevented deferrals");
            messages.Add(Messages.Timeout, "Exceeded server owners maximum loading time threshold");
            messages.Add(Messages.QueueCount, "[Queue: {0}]");
            messages.Add(Messages.Symbols, "Player name of '{0}' contains invalid characters, please change your player name in FiveM or Steam Settings.\nThe following characters are allowed A-Z, a-z, 0-9 and -_[].#");
            messages.Add(Messages.NoName, "Player name could not be found.");
        }
    }

    public enum Messages
    {
        Gathering,
        License,
        Steam,
        Banned,
        Whitelist,
        Queue,
        PriorityQueue,
        Canceled,
        Error,
        Timeout,
        QueueCount,
        Symbols,
        BlackListedName,
        NoName
    }
    public enum SessionState
    {
        Queue,
        Grace,
        Loading,
        Active,
    }
    public enum Reserved
    {
        Reserved = 1,
        Public
    }
}