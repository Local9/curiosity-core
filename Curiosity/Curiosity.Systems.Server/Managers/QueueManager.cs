using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Managers
{
    public class QueueManager : Manager<QueueManager>
    {
        static Dictionary<Messages, string> messages = new Dictionary<Messages, string>();

        static Regex regex = new Regex(@"^[ A-Za-z0-9-_.#\[\]]{1,32}$");
        static Regex blacklistedNames = new Regex(@"\b(admin|nigga|nigger|administrator|moderator|staff|n1gg3|n1g|n1gg3r)\b");

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
        static int reservedTypeOneSlots = 2;
        static int reservedTypeTwoSlots = 0;
        static int reservedTypeThreeSlots = 0;
        static int publicTypeSlots = 0;
        static int maxSession = 32;

        static long serverSetupTimer = API.GetGameTimer();
        static long forceWait = (1000 * 30);
        static bool IsServerQueueReady = false;

        // Concurrent Values
        static ConcurrentDictionary<string, SessionState> session = new ConcurrentDictionary<string, SessionState>();
        static ConcurrentDictionary<string, Player> sentLoading = new ConcurrentDictionary<string, Player>();
        static ConcurrentDictionary<string, int> priority = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<string, int> index = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<string, DateTime> timer = new ConcurrentDictionary<string, DateTime>();
        // Slots
        static ConcurrentDictionary<string, Reserved> reserved = new ConcurrentDictionary<string, Reserved>();
        static ConcurrentDictionary<string, Reserved> slotTaken = new ConcurrentDictionary<string, Reserved>();
        // Concurrent Queues:
        static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        static ConcurrentQueue<string> priorityQueue = new ConcurrentQueue<string>();
        static ConcurrentQueue<string> newQueue = new ConcurrentQueue<string>();
        static ConcurrentQueue<string> newPriorityQueue = new ConcurrentQueue<string>();

        public override void Begin()
        {
            SetupConvars();

            Logger.Debug($"[QueueManager] Begin");

            Curiosity.EventRegistry["playerConnecting"] += new Action<Player, string, CallbackDelegate, ExpandoObject>(OnConnect);
            Curiosity.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
            Curiosity.EventRegistry["onResourceStop"] += new Action<string>(OnResourceStop);
            Curiosity.EventRegistry["curiosity:Server:Queue:PlayerConnected"] += new Action<Player>(OnPlayerActivated);

            Curiosity.AttachTickHandler(QueueCycle);
            Curiosity.AttachTickHandler(SetupTimer);

            serverSetupTimer = API.GetGameTimer();
        }

        private async void OnConnect([FromSource] Player player, string name, CallbackDelegate kickManager,
            dynamic deferrals)
        {
            string license = player.Identifiers["license"];
            string discordIdStr = player.Identifiers["discord"];
            string steamId = player.Identifiers["steamId"];

            while (!IsServerQueueReady)
            {
                await BaseScript.Delay(1000);
                deferrals.update("Awaiting Server Startup...");
            }

            deferrals.update($"{messages[Messages.Gathering]}");

            if (string.IsNullOrEmpty(license)) // No License, No Gameplay
            {
                deferrals.done($"{messages[Messages.License]}");
                API.CancelEvent();
                return;
            }

            if (!regex.IsMatch(player.Name))
            {
                deferrals.done($"{string.Format(messages[Messages.Symbols], player.Name)}");
                API.CancelEvent();
                return;
            }

            if (blacklistedNames.IsMatch(player.Name.ToLower()))
            {
                deferrals.done($"The username of '{player.Name}' is blacklisted, please change your username and try to rejoin.");
                API.CancelEvent();
                return;
            }

            ulong discordId = 0;
            ulong.TryParse(discordIdStr, out discordId);

            CuriosityUser curiosityUser = await MySQL.Store.UserDatabase.Get(license, player, discordId);

            Logger.Info($"Curiosity Queue Manager : {curiosityUser.UserRole} {curiosityUser.LastName} Connecting [{discordId}]");

            await BaseScript.Delay(10);

            DiscordClient dc = new DiscordClient();
            await dc.CheckDiscordIdIsInGuild(player, discordId);

            Logger.Info($"Curiosity Queue Manager : {curiosityUser.UserRole} {curiosityUser.LastName} is a member of the Discord");

            if (curiosityUser.Banned)
            {
                string time = $"until {curiosityUser.BannedUntil}";
                if (curiosityUser.BannedPerm)
                    time = "permanently.";

                deferrals.done(string.Format($"{messages[Messages.Banned]}", time));
                API.CancelEvent();
                return;
            }

            if (!curiosityUser.IsStaff)
            {
                if (CuriosityPlugin.IsMaintenanceActive)
                {
                    deferrals.done($"Curiosity Queue Manager : This server is in a testing state, please make sure you are connecting to the correct server or use the links provided at {CuriosityPlugin.WebsiteUrl}.");
                    API.CancelEvent();
                    return;
                }
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
                    Logger.Success($"Curiosity Queue Manager : Staff Member {curiosityUser.LastName} added to Priority Queue");
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
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : NEW -> QUEUE -> (Public) {player.Name} [{license}]"); }
                }
                else
                {
                    newPriorityQueue.Enqueue(license);
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : NEW -> QUEUE -> (Priority) {player.Name} [{license}]"); }
                }
            }

            if (!session[license].Equals(SessionState.Queue))
            {
                UpdateTimer(license);
                session.TryGetValue(license, out SessionState oldState);
                session.TryUpdate(license, SessionState.Loading, oldState);
                deferrals.done();
                if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> LOADING -> (Grace) {player.Name} [{license}]"); }
                API.CancelEvent();
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
                if (player?.EndPoint == null)
                {
                    UpdateTimer(license);
                    deferrals.done($"{messages[Messages.Canceled]}");
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : QUEUE -> CANCELED -> {license}"); }
                    return;
                }
                RemoveFrom(license, false, false, true, false, false, false);
                await BaseScript.Delay(5000);
            }
            await BaseScript.Delay(500);

            deferrals.done();
        }

        async Task SetupTimer()
        {
            try
            {
                if (CuriosityPlugin.IsMaintenanceActive)
                {
                    IsServerQueueReady = true;
                    Curiosity.DetachTickHandler(SetupTimer);
                    Logger.Verbose("[MAINTENANCE] Server Queue is ready.");
                }
                else
                {
                    if ((API.GetGameTimer() - serverSetupTimer) > forceWait)
                    {
                        IsServerQueueReady = true;
                        Curiosity.DetachTickHandler(SetupTimer);
                        Logger.Verbose("Server Queue is ready.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Verbose($"SetupTimer() -> {ex.Message}");
            }
            await Task.FromResult(0);
        }

        async Task QueueCycle()
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
                    string license = j.Key;
                    SessionState state = j.Value;
                    PlayerList players = CuriosityPlugin.PlayersList;
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
                            else
                            {
                                if (sentLoading.ContainsKey(license) && players.FirstOrDefault(i => i.Identifiers["license"] == license) != null)
                                {
                                    BaseScript.TriggerEvent("curiosity:Server:Queue:NewLoading", sentLoading[license]);
                                    sentLoading.TryRemove(license, out Player oldPlayer);
                                }
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
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : UpdateStates()");
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
                    int openReservedTypeOneSlots = reservedTypeOneSlots - slotTaken.Count(j => j.Value == Reserved.Reserved1);
                    int openReservedTypeTwoSlots = reservedTypeTwoSlots - slotTaken.Count(j => j.Value == Reserved.Reserved2);
                    int openReservedTypeThreeSlots = reservedTypeThreeSlots - slotTaken.Count(j => j.Value == Reserved.Reserved3);

                    switch (k.Value)
                    {
                        case Reserved.Reserved1:
                            if (openReservedTypeOneSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved1))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved1, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved1"); }
                            }
                            else if (openReservedTypeTwoSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved2))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved2, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved2"); }
                            }
                            else if (openReservedTypeThreeSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved3))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved3, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved3"); }
                            }
                            break;

                        case Reserved.Reserved2:
                            if (openReservedTypeTwoSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved2))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved2, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved2"); }
                            }
                            else if (openReservedTypeThreeSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved3))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved3, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved3"); }
                            }
                            break;

                        case Reserved.Reserved3:
                            if (openReservedTypeThreeSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved3))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved3, oldReserved);
                                }
                                if (stateChangeMessages) { Logger.Verbose($"Assigned {k.Key} to Reserved3"); }
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
                if (reserved.ContainsKey(license) && reserved[license] == Reserved.Reserved1 && slotTaken.Count(j => j.Value == Reserved.Reserved1) < reservedTypeOneSlots)
                { NewLoading(license, Reserved.Reserved1); return true; }
                else if (reserved.ContainsKey(license) && (reserved[license] == Reserved.Reserved1 || reserved[license] == Reserved.Reserved2) && slotTaken.Count(j => j.Value == Reserved.Reserved2) < reservedTypeTwoSlots)
                { NewLoading(license, Reserved.Reserved2); return true; }
                else if (reserved.ContainsKey(license) && (reserved[license] == Reserved.Reserved1 || reserved[license] == Reserved.Reserved2 || reserved[license] == Reserved.Reserved3) && slotTaken.Count(j => j.Value == Reserved.Reserved3) < reservedTypeThreeSlots)
                { NewLoading(license, Reserved.Reserved3); return true; }
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

        void OnPlayerDropped([FromSource] Player source, string message)
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
                else
                {
                    session.TryGetValue(license, out SessionState oldState);
                    session.TryUpdate(license, SessionState.Grace, oldState);
                    if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> GRACE -> {license}"); }
                    UpdateTimer(license);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : PlayerDropped()");
            }
        }

        async void OnPlayerActivated([FromSource] Player source)
        {
            try
            {
                await BaseScript.Delay(0);
                string license = source.Identifiers["license"];
                string discordId = source.Identifiers["discord"];
                if (!session.ContainsKey(license))
                {
                    session.TryAdd(license, SessionState.Active);
                    return;
                }
                session.TryGetValue(license, out SessionState oldState);
                session.TryUpdate(license, SessionState.Active, oldState);
                if (stateChangeMessages) { Logger.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> ACTIVE -> {license}"); }

                if (discordId == "191686898450825217")
                    BaseScript.TriggerClientEvent("curiosity:Client:Player:Developer:Online");

                BaseScript.TriggerEvent("environment:train:activate");
            }
            catch (Exception ex)
            {
                Logger.Error($"Curiosity Queue Manager : PlayerActivated() {ex}");
            }
        }

        void RemoveFrom(string license, bool doSession, bool doIndex, bool doTimer, bool doPriority, bool doReserved, bool doSlot)
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

            if (API.GetConvar("onesync_enabled", "false") == "true")
            {
                Logger.Warn($"Curiosity Queue Manager : Server reports that OneSync is enabled. Ignoring regular 32 player limit, set slots to {maxSession}.");
            }
            else
            {
                if (maxSession > 32) { maxSession = 32; }
            }
            API.ExecuteCommand($"sv_maxclients {maxSession}");

            loadTime = API.GetConvarInt("queue_loading_timeout", loadTime);
            graceTime = API.GetConvarInt("queue_reconnect_timeout", graceTime);
            queueGraceTime = API.GetConvarInt("queue_cancel_timeout", queueGraceTime);
            reservedTypeOneSlots = API.GetConvarInt("queue_type_1_reserved_slots", reservedTypeOneSlots);
            reservedTypeTwoSlots = API.GetConvarInt("queue_type_2_reserved_slots", reservedTypeTwoSlots);
            reservedTypeThreeSlots = API.GetConvarInt("queue_type_3_reserved_slots", reservedTypeThreeSlots);
            publicTypeSlots = maxSession - reservedTypeOneSlots - reservedTypeTwoSlots - reservedTypeThreeSlots;

            Logger.Info($"Queue Settings -> queue_max_session_slots {maxSession}");
            Logger.Info($"Queue Settings -> queue_loading_timeout {loadTime} mins");
            Logger.Info($"Queue Settings -> queue_reconnect_timeout {graceTime} mins");
            Logger.Info($"Queue Settings -> queue_cancel_timeout {queueGraceTime} mins");
            Logger.Info($"Queue Settings -> queue_type_1_reserved_slots {reservedTypeOneSlots}");
            Logger.Info($"Queue Settings -> queue_type_2_reserved_slots {reservedTypeTwoSlots}");
            Logger.Info($"Queue Settings -> queue_type_3_reserved_slots {reservedTypeThreeSlots}");
            Logger.Info($"Queue Settings -> Final Public Slots: {publicTypeSlots}");
            
            SetupMessages();
            
            Logger.Success($"Queue Configuration Completed");
        }

        void SetupMessages()
        {
            messages.Add(Messages.Gathering, "Gathering queue information");
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
        BlackListedName
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
        Reserved1 = 1,
        Reserved2,
        Reserved3,
        Public
    }
}