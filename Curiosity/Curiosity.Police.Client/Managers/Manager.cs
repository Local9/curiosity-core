using Curiosity.Police.Client.Events;

namespace Curiosity.Police.Client.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return PluginManager.Instance.GetManager<T>() ?? (!PluginManager.Instance.IsLoadingManager<T>()
                       ? (T)PluginManager.Instance.LoadManager(typeof(T))
                       : null);
        }

        public PluginManager Curiosity { get; set; }
        public EventSystem EventSystem => EventSystem.GetModule();

        protected Manager()
        {
            Curiosity = PluginManager.Instance;
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}