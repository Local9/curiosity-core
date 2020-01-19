using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

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
            SetupMessages();

            Curiosity.EventRegistry["playerConnecting"] +=
                new Action<Player, string, CallbackDelegate, ExpandoObject>(OnConnect);
        }

        private async void OnConnect([FromSource] Player player, string name, CallbackDelegate kickManager,
            dynamic deferrals)
        {
            string license = player.Identifiers["license"];
            string discordId = player.Identifiers["discord"];
            string steamId = player.Identifiers["steamId"];

            while (!CuriosityPlugin.ServerReady)
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
                return;
            }
        }

        static void SetupConvars()
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

            Logger.Verbose($"Queue Settings -> queue_max_session_slots {maxSession}");
            Logger.Verbose($"Queue Settings -> queue_loading_timeout {loadTime} mins");
            Logger.Verbose($"Queue Settings -> queue_reconnect_timeout {graceTime} mins");
            Logger.Verbose($"Queue Settings -> queue_cancel_timeout {queueGraceTime} mins");
            Logger.Verbose($"Queue Settings -> queue_type_1_reserved_slots {reservedTypeOneSlots}");
            Logger.Verbose($"Queue Settings -> queue_type_2_reserved_slots {reservedTypeTwoSlots}");
            Logger.Verbose($"Queue Settings -> queue_type_3_reserved_slots {reservedTypeThreeSlots}");
            Logger.Verbose($"Queue Settings -> Final Public Slots: {publicTypeSlots}");
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