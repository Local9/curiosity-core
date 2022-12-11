using Curiosity.Framework.Client.Engine;
using Curiosity.Framework.Client.Managers;
using Logger;
using System.Reflection;

namespace Curiosity.Framework.Client
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public Log Logger;
        internal SoundEngine SoundEngine;
        public Dictionary<Type, object> Managers { get; } = new();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new();
        public List<Type> RegisteredTickHandlers { get; set; } = new();

        public GameInterface.Hud Hud;
        public static Random Random => new(GetGameTimer());

        //   public readonly DiscordRichPresence DiscordRichPresence =
        //new DiscordRichPresence(
        //    GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_asset", 0),
        //    GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_asset_text", 0)
        //    )
        //{
        //    SmallAsset = GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_small_asset", 0),
        //    SmallAssetText = GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_small_asset_text", 0),
        //    Status = "Connecting..."
        //};

        public PluginManager()
        {
            try
            {
                Instance = this;
                Hud = new();
                Logger = new Log();

                SoundEngine = new SoundEngine();

                EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
                EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };

                OnLoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{Log.DARK_RED}{ex.Message}");
                Debug.WriteLine($"{Log.DARK_RED}{ex}");
            }
        }

        private async Task OnLoadAsync()
        {
            try
            {
                List<MethodInfo> managers = Assembly.GetExecutingAssembly().GetExportedTypes()
                    .SelectMany(self => self.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                    .Where(self => self.GetCustomAttribute(typeof(TickHandler), false) != null).ToList();

                managers.ForEach(self =>
                {
                    try
                    {
                        var type = self.DeclaringType;
                        if (type == null) return;

                        Logger.Debug($"[TickHandlers] {type.Name}::{self.Name}");

                        if (!TickHandlers.ContainsKey(type))
                        {
                            TickHandlers.Add(type, new List<MethodInfo>());
                        }

                        TickHandlers[type].Add(self);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{ex}");
                        BaseScript.TriggerServerEvent("user:log:exception", $"Error with Tick; {ex.Message}", ex);
                    }
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

                Logger.Info($"[Managers] Successfully loaded in {loaded} manager(s)!");

                AttachTickHandlers(this);

                Logger.Info("Load method has been completed.");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        private void OnResourceStart(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;
        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;
        }

        public void AddEventHandler(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
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

        public void AttachTickHandlers(object instance)
        {
            TickHandlers.TryGetValue(instance.GetType(), out var methods);

            methods?.ForEach(async self =>
            {
                try
                {
                    var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                    if (handler.SessionWait)
                    {
                        await Session.Loading();
                    }

                    Logger.Debug($"AttachTickHandlers -> {self.Name}");

                    Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                    RegisteredTickHandlers.Add(instance.GetType());
                }
                catch (Exception ex)
                {
                    Logger.Fatal($"AttachTickHandlers");
                    Logger.Fatal($"{ex}");
                }
            });
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
    }
}
