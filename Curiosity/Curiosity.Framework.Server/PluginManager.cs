using Curiosity.Framework.Server.Attributes;
using Curiosity.Framework.Server.Events;
using Curiosity.Framework.Server.Managers;
using Curiosity.Framework.Server.Models.Database;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Curiosity.Framework.Server
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public static PlayerList PlayerList;
        public static ConcurrentDictionary<int, ClientId> UserSessions = new();

        public ServerGateway Events;
        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportDictionary => Exports;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();
        public static bool IsOneSyncEnabled => GetConvar("onesync", "off") != "off";
        public static bool IsServerReady = false;


        internal static int ServerID
        {
            get
            {
                int _ServerId = GetConvarInt("server_id", 0);
                if (_ServerId == 0)
                {
                    Logger.CriticalError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    Logger.CriticalError("! Convar 'server_id' is not set or is not a number! !");
                    Logger.CriticalError("!!! Please set this value and restart the server! !!!");
                    Logger.CriticalError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
                return _ServerId;
            }
        }

        public PluginManager()
        {
            Logger.Trace($"CURIOSITY INITIATION");
            Instance = this;
            Events = new ServerGateway();

            Load();
        }

        public async void Load()
        {
            PlayerList = Players;

            Assembly.GetExecutingAssembly().GetExportedTypes()
                .SelectMany(self => self.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                .Where(self => self.GetCustomAttribute(typeof(TickHandler), false) != null).ToList()
                .ForEach(self =>
                {
                    var type = self.DeclaringType;

                    if (type == null) return;

                    if (!TickHandlers.ContainsKey(type))
                    {
                        TickHandlers.Add(type, new List<MethodInfo>());
                    }

                    Logger.Debug($"[TickHandlers] {type.Name}::{self.Name}");

                    TickHandlers[type].Add(self);
                });

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

            bool databaseTest = await Database.DapperDatabase<bool>.GetSingleAsync("select 1;");
            if (databaseTest)
            {
                Logger.Trace($"Database Connection Test Successful!");
            }
            else
            {
                Logger.CriticalError($"Database Connection Test Failed!");
            }

            IsServerReady = !IsServerReady;
            Logger.Trace($"CURIOSITY INITIATED: {loaded} Managers Initiated");
        }

        public object LoadManager(Type type)
        {
            if (GetManager(type) != null) return null;

            Logger.Debug($"Loading in manager of type `{type.FullName}`");

            Managers.Add(type, default(Type));

            var instance = Activator.CreateInstance(type);

            AttachTickHandlers(instance);
            Managers[type] = instance;

            return instance;
        }

        public bool IsLoadingManager<T>() where T : Manager<T>, new()
        {
            return !(Managers.FirstOrDefault(self => self.Key == typeof(T)).Value is bool);
        }

        public object GetManager(Type type)
        {
            return Managers.FirstOrDefault(self => self.Key == type).Value;
        }

        public T GetManager<T>() where T : Manager<T>, new()
        {
            return (T)Managers.FirstOrDefault(self => self.Key == typeof(T)).Value;
        }

        public void AttachTickHandlers(object instance)
        {
            TickHandlers.TryGetValue(instance.GetType(), out var methods);

            methods?.ForEach(self =>
            {
                var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                RegisteredTickHandlers.Add(instance.GetType());
            });
        }

        public void AddEventHandler(string eventName, Delegate @delegate)
        {
            Logger.Debug($"Registered Legacy Event Handler '{eventName}'");
            EventHandlers.Add(eventName, @delegate);
        }

        public static Player GetPlayerFromId(int handle)
        {
            return Instance.Players[handle];
        }

        public static User GetUserFromId(string handle)
        {
            if (int.TryParse(handle, out int iHandle))
                return ToClient(iHandle).User;
            return null;
        }

        public static ClientId ToClient(int handle)
        {
            if (!UserSessions.ContainsKey(handle)) return null;
            return UserSessions[handle];
        }

        internal static async Task IsReady()
        {
            while (!IsServerReady)
            {
                await BaseScript.Delay(100);
            }
        }
    }
}
