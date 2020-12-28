using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;

namespace Curiosity.Core.Server.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return PluginManager.Instance.GetManager<T>() ?? (!PluginManager.Instance.IsLoadingManager<T>()
                       ? (T)PluginManager.Instance.LoadManager(typeof(T))
                       : null);
        }

        public PluginManager Instance { get; set; }
        public EventSystem EventSystem { get; set; }

        protected Manager()
        {
            Instance = PluginManager.Instance;
            EventSystem = EventSystem.GetModule();
        }

        public virtual void Begin()
        {
            Logger.Debug($"[Manager] Begin");
            // Ignored
        }
    }
}