using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Managers;
using Curiosity.Systems.Server.MySQL;
using Curiosity.Systems.Server.Web;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server
{
    public class CuriosityPlugin : BaseScript
    {
        const string CURIOSITY_VERSION = "v2.0.0.0001";
        private const string CONVAR_MISSING = "MISSING";

        public static CuriosityPlugin Instance { get; private set; }
        public static PlayerList PlayersList { get; private set; }
        public static int MaximumPlayers { get; } = 32;
        public static int ServerId { get; private set; }
        public static int SpawnLocationId { get; private set; }
        public static int SaveInterval { get; } = 1000 * 60 * 3;
        public static bool IsDebugging { get; private set; }
        public static bool IsMaintenanceActive { get; private set; }
        public static bool ServerReady { get; private set; }
        public static string DatabaseConnectionString { get; private set; }
        public static ulong DiscordGuildId { get; private set; }
        public static string DiscordBotKey { get; private set; }
        public static string DiscordUrl { get; private set; }
        public static string WebsiteUrl { get; private set; }
        public static Dictionary<string, CuriosityUser> ActiveUsers { get; } = new Dictionary<string, CuriosityUser>();
        public long LastSave { get; set; } = Date.Timestamp;

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();

        public CuriosityPlugin()
        {
            Logger.Info("[CuriosityPlugin] Loading...");

            Instance = this;

            PlayersList = Players;
            SetupConvars();

            ServerReady = false;

            async Task DatabaseTest()
            {
                try
                {
                    Logger.Info("Running DB Connection Test...");
                    
                    await BaseScript.Delay(100);
                    
                    DetachTickHandler(DatabaseTest);

                    using (var db = new MySqlDatabase())
                    {
                        await db.Connection.OpenAsync();

                        using (var cmd = new MySqlCommand("SELECT 'Success' as Success", db.Connection))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (reader.GetString(0) == "Success")
                                        Logger.Success("DB Connection Test Successful");
                                }
                            }
                        }
                    }

                    async Task LoadTask()
                    {
                        DetachTickHandler(LoadTask);
                        Load();
                    }
                    AttachTickHandler(LoadTask);
                }
                catch (Exception ex)
                {
                    Logger.Error("!!! Critical Error trying to test Database Connection !!!");
                    Logger.Error($"Message: {ex.Message}");
                }
            }

            AttachTickHandler(DatabaseTest);
        }

        private async void SetupConvars()
        {
            try
            {
                IsDebugging = API.GetConvar("diagnostics_debug", "false") == "true";

                if (IsDebugging)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"------------ DEBUG ACTIVE --------------");
                    Logger.Warn($"----------------------------------------");
                }

                IsMaintenanceActive = API.GetConvar("server_live", "false") == "false";

                if (IsMaintenanceActive)
                {
                    Logger.Warn($"----------------------------------------");
                    Logger.Warn($"--------- MAINTENANCE ACTIVE -----------");
                    Logger.Warn($"----------------------------------------");
                }

                ulong defDiscordGuildId = 0;
                if (ulong.TryParse(API.GetConvar("discord_guild", "0"), out defDiscordGuildId))
                {
                    DiscordGuildId = defDiscordGuildId;
                    Logger.Success($"Discord Guild ID: {DiscordGuildId}");
                }
                else
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("! Convar 'discord_guild' is not set or is not a number! !");
                        Logger.Error("!!!!! Please set this value and restart the server! !!!!!");
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        await BaseScript.Delay(3000);
                    }
                }

                DiscordBotKey = API.GetConvar("discord_bot", CONVAR_MISSING);

                if (DiscordBotKey == CONVAR_MISSING)
                {
                    Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    Logger.Error("!!!!!!!!!!! Convar 'discord_bot' is not set! !!!!!!!!!!!!");
                    Logger.Error("!!!!! Please set this value and restart the server! !!!!!");
                    Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }

                DatabaseConnectionString = API.GetConvar("mysql_connection_string", "Host=localhost;Port=3306;Username=root;Password=;Database=curiosity;");

                if (IsDebugging)
                    Logger.Debug($"Database String: {DatabaseConnectionString}");

                ServerId = API.GetConvarInt("server_id", 0);
                if (ServerId == 0)
                {
                    while (true)
                    {
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Logger.Error("! Convar 'server_id' is not set or is not a number! !");
                        Logger.Error("!!! Please set this value and restart the server! !!!");
                        Logger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        await BaseScript.Delay(3000);
                    }
                }

                DiscordUrl = API.GetConvar("discord_url", "discord_url not set");
                API.SetConvarServerInfo("Discord", DiscordUrl);

                WebsiteUrl = API.GetConvar("website_url", "website_url not set");
                API.SetConvarServerInfo("Website", WebsiteUrl);
                API.SetGameType(API.GetConvar("game_type", "game_type not set"));
                API.SetMapName("Life V - Curiosity Framework");

                SpawnLocationId = API.GetConvarInt("starting_location_id", 1);

                string tags = API.GetConvar("tags", string.Empty);
                string[] tagArr = tags.Split(',');
                string curiosity = "Curiosity";

                if (tagArr.Length > 0)
                {
                    API.SetConvar("tags", $"{tags}, {curiosity}");
                }
                else
                {
                    API.SetConvar("tags", $"{curiosity}");
                }
                API.SetConvarServerInfo("Curiosity", CURIOSITY_VERSION);
            }
            catch (Exception ex)
            {
                ErrorText errorText = new ErrorText();
                errorText.PrintErrorText();
            }
        }

        private void Load()
        {
            var loaded = 0;

            foreach (var type in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (type.BaseType == null) continue;
                if (!type.BaseType.IsGenericType) continue;

                var generic = type.BaseType.GetGenericTypeDefinition();

                if (generic != typeof(Manager<>) || type == typeof(Manager<>)) continue;

                LoadManager(type);

                loaded++;
            }

            foreach (var manager in Managers)
            {
                var method = manager.Key.GetMethod("Begin", BindingFlags.Public | BindingFlags.Instance);

                method?.Invoke(manager.Value, null);
            }

            Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

            EventRegistry["rconCommand"] += new Action<string, List<object>>(OnRconCommand);

            ServerReady = true;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        private async void OnRconCommand(string commandName, List<object> args)
        {
            Logger.Debug($"RCON Command: {commandName}");
            API.CancelEvent();
        }

        public object LoadManager(Type type)
        {
            if (GetManager(type) != null) return null;

            Logger.Debug($"Loading in manager of type `{type.FullName}`");

            Managers.Add(type, default(Type));

            var instance = Activator.CreateInstance(type);

            Managers[type] = instance;

            return instance;
        }

        public bool IsLoadingManager<T>() where T : Manager<T>, new()
        {
            return Managers.FirstOrDefault(self => self.Key == typeof(T)).Value is bool == false;
        }

        public object GetManager(Type type)
        {
            return Managers.FirstOrDefault(self => self.Key == type).Value;
        }

        public T GetManager<T>() where T : Manager<T>, new()
        {
            return (T)Managers.FirstOrDefault(self => self.Key == typeof(T)).Value;
        }

        public async Task<string> RequestHttp(string url, string json, Dictionary<string, string> headers,
            string method = "GET")
        {
            var request = new Request();

            return (await request.Http(url, method, json, headers)).content;
        }
    }
}
