using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Interface.Client.Diagnostics;
using Curiosity.Interface.Client.Events;
using Curiosity.Interface.Client.Extensions;
using Curiosity.Interface.Client.Managers;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client
{
    public class CuriosityPlugin : BaseScript
    {
        public const string DECOR_PED_OWNER = "PED_OWNER";

        public static CuriosityPlugin Instance { get; private set; }
        public static Random Rand = new Random();
        public static int MaximumPlayers { get; } = 32;

        public PlayerList PlayerList;

        public EventHandlerDictionary EventRegistry => EventHandlers;
        public ExportDictionary ExportRegistry => Exports;
        public Dictionary<Type, object> Managers { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, List<MethodInfo>> TickHandlers { get; set; } = new Dictionary<Type, List<MethodInfo>>();
        public List<Type> RegisteredTickHandlers { get; set; } = new List<Type>();

        public CuriosityPlugin()
        {
            Logger.Info("[Curiosity]: Constructor Call from CitizenFX - BaseScript");

            PlayerList = Players;

            Instance = this;
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
                var handler = (TickHandler)self.GetCustomAttribute(typeof(TickHandler));

                Tick += (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), instance, self);

                RegisteredTickHandlers.Add(instance.GetType());
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

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        public void AttachNuiHandler(string pipe, EventCallback callback)
        {
            API.RegisterNuiCallbackType(pipe);

            EventHandlers[$"__cfx_nui:{pipe}"] += new Action<ExpandoObject, CallbackDelegate>((body, result) =>
            {
                var metadata = new EventMetadata();
                var properties = (IDictionary<string, object>)body;

                if (properties != null)
                {
                    foreach (var entry in properties)
                    {
                        if (!int.TryParse(entry.Key, out var index))
                        {
                            Logger.Info(
                                $"[Nui] [{pipe}] Payload `{entry.Key}` is not a number and therefore not written to.");

                            continue;
                        }

                        metadata.Write(index, entry.Value);
                    }
                }

                if (callback.GetType() == typeof(AsyncEventCallback))
                {
                    ((AsyncEventCallback)callback).AsyncTask(metadata);
                }
                else
                {
                    callback.Task(metadata);
                }
            });
        }
    }
}
