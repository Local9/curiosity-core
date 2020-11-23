using Curiosity.MissionManager.Client.Events;

namespace Curiosity.MissionManager.Client.Managers
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
        public static EventSystem EventSystem => EventSystem.GetModule();

        protected Manager()
        {
            Instance = PluginManager.Instance;
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}