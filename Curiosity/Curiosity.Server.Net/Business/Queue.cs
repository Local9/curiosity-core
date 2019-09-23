using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Curiosity.Shared.Server.net.Helpers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;
using System.IO;

namespace Curiosity.Server.net.Business
{
    class Queue
    {
        // service requirements
        static Server server = Server.GetInstance();
        static Regex regex = new Regex(@"^[ A-Za-z0-9-_.#\[\]]{1,32}$");
        static Regex blacklistedNames = new Regex(@"\b(admin|nigga|nigger|administrator|moderator|staff)\b");
        static string resourceName = API.GetCurrentResourceName();
        static string resourcePath = $"resources/{API.GetResourcePath(resourceName).Substring(API.GetResourcePath(resourceName).LastIndexOf("//") + 2)}";
        static Dictionary<Messages, string> messages = new Dictionary<Messages, string>();
        static bool isServerQueueReady = false;
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

        public static void Init()
        {
            SetupConvars();
            SetupMessages();

            server.RegisterEventHandler("onResourceStop", new Action<string>(OnResourceStop));
            server.RegisterEventHandler("playerConnecting", new Action<CitizenFX.Core.Player, string, dynamic, dynamic>(PlayerConnecting));
            server.RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(PlayerDropped));
            server.RegisterEventHandler("curiosity:Server:Queue:PlayerConnected", new Action<CitizenFX.Core.Player>(PlayerActivated));
            server.RegisterTickHandler(QueueCycle);
            server.RegisterTickHandler(SetupTimer);

            serverSetupTimer = API.GetGameTimer();
        }

        static async Task SetupTimer()
        {
            try
            {
                if (!Server.isLive)
                {
                    isServerQueueReady = true;
                    server.DeregisterTickHandler(SetupTimer);
                    Log.Verbose("Server Queue is ready.");
                }
                else
                {
                    if ((API.GetGameTimer() - serverSetupTimer) > forceWait)
                    {
                        isServerQueueReady = true;
                        server.DeregisterTickHandler(SetupTimer);
                        Log.Verbose("Server Queue is ready.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Verbose($"SetupTimer() -> {ex.Message}");
            }
            await Task.FromResult(0);
        }

        static void SetupConvars()
        {
            stateChangeMessages = API.GetConvar("queue_enable_console_messages", "true") == "true";
            maxSession = API.GetConvarInt("queue_max_session_slots", maxSession);

            if (API.GetConvar("onesync_enabled", "false") == "true")
            {
                Log.Warn($"Curiosity Queue Manager : Server reports that OneSync is enabled. Ignoring regular 32 player limit, set slots to {maxSession}.");
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

            Log.Verbose($"Queue Settings -> queue_max_session_slots {maxSession}");
            Log.Verbose($"Queue Settings -> queue_loading_timeout {loadTime} mins");
            Log.Verbose($"Queue Settings -> queue_reconnect_timeout {graceTime} mins");
            Log.Verbose($"Queue Settings -> queue_cancel_timeout {queueGraceTime} mins");
            Log.Verbose($"Queue Settings -> queue_type_1_reserved_slots {reservedTypeOneSlots}");
            Log.Verbose($"Queue Settings -> queue_type_2_reserved_slots {reservedTypeTwoSlots}");
            Log.Verbose($"Queue Settings -> queue_type_3_reserved_slots {reservedTypeThreeSlots}");
            Log.Verbose($"Queue Settings -> Final Public Slots: {publicTypeSlots}");
        }

        static void SetupMessages()
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
            messages.Add(Messages.Symbols, "Invalid characters in your name. The following are allowed A-Z, a-z, 0-9 and _[].#|");
        }
        
        static bool IsEverythingReady()
        {
            return isServerQueueReady;
        }

        static async void PlayerConnecting([FromSource]CitizenFX.Core.Player player, string playerName, dynamic denyWithReason, dynamic deferrals)
        {
            try
            {
                deferrals.defer();

                await Server.Delay(0);

                if (!IsEverythingReady())
                    deferrals.update($"Checking server startup");

                //string adaptiveWelcomeCard = string.Empty;
                //string path = Path.Combine($@"{resourcePath}/data/welcome.json");

                //if (File.Exists(path))
                //{
                //    adaptiveWelcomeCard = File.ReadAllText(path);
                //}

                while (!IsEverythingReady()) { await Server.Delay(0); }
                deferrals.update($"{messages[Messages.Gathering]}");

                string license = player.Identifiers["license"];

                if (license == null) { deferrals.done($"{messages[Messages.License]}"); return; }

                if (!regex.IsMatch(playerName)) { deferrals.done($"{messages[Messages.Symbols]}"); return; }

                if (blacklistedNames.IsMatch(playerName)) {
                    deferrals.done($"The username of '{playerName}' is blocked. Please note we can see the name from the FiveM settings options or Steam.");
                    return;
                }

                if (playerName.Contains(".com") || playerName.Contains(".net") || playerName.Contains(".org") || playerName.Contains(".co."))
                {
                    deferrals.done($"The username of '{playerName}' is blocked. Please note we can see the name from the FiveM settings options or Steam.");
                    return;
                }

                GlobalEntity.User user = await Database.DatabaseUsers.GetUser(license, player);

                await Server.Delay(10);

                if (user.Banned)
                {
                    string time = $"until {user.BannedUntil}";
                    if (user.BannedPerm)
                        time = "permanently.";

                    deferrals.done(string.Format($"{messages[Messages.Banned]}", time));
                }

                if (sentLoading.ContainsKey(license))
                {
                    sentLoading.TryRemove(license, out Player oldPlayer);
                }
                sentLoading.TryAdd(license, player);

                if (user.QueuePriority > 0)
                {
                    if (!priority.TryAdd(license, user.QueuePriority))
                    {
                        priority.TryGetValue(license, out int oldPriority);
                        priority.TryUpdate(license, user.QueuePriority, oldPriority);
                    }
                }

                if (session.TryAdd(license, SessionState.Queue))
                {
                    if (!priority.ContainsKey(license))
                    {
                        newQueue.Enqueue(license);
                        if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : NEW -> QUEUE -> (Public) {license}"); }
                    }
                    else
                    {
                        newPriorityQueue.Enqueue(license);
                        if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : NEW -> QUEUE -> (Priority) {license}"); }
                    }
                }

                if (!session[license].Equals(SessionState.Queue))
                {
                    UpdateTimer(license);
                    session.TryGetValue(license, out SessionState oldState);
                    session.TryUpdate(license, SessionState.Loading, oldState);
                    deferrals.done();
                    if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> LOADING -> (Grace) {license}"); }
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
                        if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : QUEUE -> CANCELED -> {license}"); }
                        return;
                    }
                    RemoveFrom(license, false, false, true, false, false, false);
                    await Server.Delay(5000);
                }
                await Server.Delay(500);

                //if (!string.IsNullOrEmpty(adaptiveWelcomeCard))
                //{
                //    deferrals.presentCard(adaptiveWelcomeCard);
                //}

                // await Server.Delay(10000);

                deferrals.done();
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : PlayerConnecting", $"{ex}");
                Log.Error($"Curiosity Queue Manager : {ex.Message}");
                deferrals.done($"{messages[Messages.Error]}"); return;
            }
        }

        static void Message()
        {

        }

        static async void StopHardcap()
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
                    await Server.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : StopHardcap", $"{ex}");
                Log.Error($"Curiosity Queue Manager : StopHardcap()");
            }
        }

        static void OnResourceStop(string name)
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : OnResourceStop()", $"{ex}");
                Log.Error($"Curiosity Queue Manager : OnResourceStop()");
            }
        }

        static async Task QueueCycle()
        {
            while (true)
            {
                try
                {
                    inPriorityQueue = PriorityQueueCount();
                    await Server.Delay(100);
                    inQueue = QueueCount();
                    await Server.Delay(100);
                    UpdateHostName();
                    UpdateStates();
                    await Server.Delay(100);
                    BalanceReserved();
                    await Server.Delay(1000);
                }
                catch (Exception ex)
                {
                    Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : QueueCycle", $"{ex}");
                    Log.Error($"Curiosity Queue Manager : QueueCycle()");
                }
            }
        }

        static void UpdateStates()
        {
            try
            {
                session.Where(k => k.Value == SessionState.Loading || k.Value == SessionState.Grace).ToList().ForEach(j =>
                {
                    string license = j.Key;
                    SessionState state = j.Value;
                    PlayerList players = Server.players;
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
                                if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : LOADING -> GRACE -> {license}"); }
                            }
                            else
                            {
                                if (sentLoading.ContainsKey(license) && players.FirstOrDefault(i => i.Identifiers["license"] == license) != null)
                                {
                                    Server.TriggerEvent("curiosity:Server:Queue:NewLoading", sentLoading[license]);
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
                                    if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : GRACE -> REMOVED -> {license}"); }
                                }
                            }
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : UpdateStates", $"{ex}");
                Log.Error($"Curiosity Queue Manager : UpdateStates()");
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
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved1"); }
                            }
                            else if (openReservedTypeTwoSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved2))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved2, oldReserved);
                                }
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved2"); }
                            }
                            else if (openReservedTypeThreeSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved3))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved3, oldReserved);
                                }
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved3"); }
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
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved2"); }
                            }
                            else if (openReservedTypeThreeSlots > 0)
                            {
                                if (!slotTaken.TryAdd(k.Key, Reserved.Reserved3))
                                {
                                    slotTaken.TryGetValue(k.Key, out Reserved oldReserved);
                                    slotTaken.TryUpdate(k.Key, Reserved.Reserved3, oldReserved);
                                }
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved3"); }
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
                                if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Reserved3"); }
                            }
                            break;
                        default:
                            if (stateChangeMessages) { Log.Verbose($"Assigned {k.Key} to Public"); }
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : BalanceReserved", $"{ex}");
                Log.Error($"Curiosity Queue Manager : BalanceReserved()");
            }
        }

        static void UpdateHostName()
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : UpdateHostName", $"{ex}");
                Log.Error($"Curiosity Queue Manager : UpdateHostName()");
            }
        }

        static int QueueCount()
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
                        if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : CANCELED -> REMOVED -> {license}"); }
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : QueueCount", $"{ex}");
                Log.Error($"Curiosity Queue Manager : QueueCount()"); return queue.Count;
            }
        }

        static bool Loading(string license)
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : Loading", $"{ex}");
                Log.Error($"Curiosity Queue Manager : Loading()"); return false;
            }
        }

        static void NewLoading(string license, Reserved slotType)
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
                    if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : QUEUE -> LOADING -> ({Enum.GetName(typeof(Reserved), slotType)}) {license}"); }
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : NewLoading", $"{ex}");
                Log.Error($"Curiosity Queue Manager : NewLoading()");
            }
        }

        static bool IsTimeUp(string license, double time)
        {
            try
            {
                if (!timer.ContainsKey(license)) { return false; }
                return timer[license].AddMinutes(time) < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : IsTimeUp", $"{ex}");
                Log.Error($"Curiosity Queue Manager : IsTimeUp()"); return false;
            }
        }

        static void UpdatePlace(string license, int place)
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : UpdatePlace", $"{ex}");
                Log.Error($"Curiosity Queue Manager : UpdatePlace()");
            }
        }

        static void UpdateTimer(string license)
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : UpdateTimer", $"{ex}");
                Log.Error($"Curiosity Queue Manager : UpdateTimer()");
            }
        }

        static int PriorityQueueCount()
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
                        if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : CANCELED -> REMOVED -> {license}"); }
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : PriorityQueueCount", $"{ex}");
                Log.Error($"Curiosity Queue Manager : PriorityQueueCount()"); return priorityQueue.Count;
            }
        }

        static void PlayerDropped([FromSource] Player source, string message)
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
                    if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : REMOVED -> {license}"); }
                }
                else
                {
                    session.TryGetValue(license, out SessionState oldState);
                    session.TryUpdate(license, SessionState.Grace, oldState);
                    if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> GRACE -> {license}"); }
                    UpdateTimer(license);
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : PlayerDropped", $"{ex}");
                Log.Error($"Curiosity Queue Manager : PlayerDropped()");
            }
        }

        static async void PlayerActivated([FromSource] Player source)
        {
            try
            {
                await Server.Delay(0);
                string license = source.Identifiers["license"];
                if (!session.ContainsKey(license))
                {
                    session.TryAdd(license, SessionState.Active);
                    return;
                }
                session.TryGetValue(license, out SessionState oldState);
                session.TryUpdate(license, SessionState.Active, oldState);
                if (stateChangeMessages) { Log.Verbose($"Curiosity Queue Manager : {Enum.GetName(typeof(SessionState), oldState).ToUpper()} -> ACTIVE -> {license}"); }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : PlayerActivated", $"{ex}");
                Log.Error($"Curiosity Queue Manager : PlayerActivated()");
            }
        }

        static void RemoveFrom(string license, bool doSession, bool doIndex, bool doTimer, bool doPriority, bool doReserved, bool doSlot)
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "Curiosity Queue Manager : RemoveFrom", $"{ex}");
                Log.Error($"Curiosity Queue Manager : RemoveFrom()");
            }
        }
    }
}
