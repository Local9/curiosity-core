using Logger;

namespace Curiosity.Framework.Client.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return PluginManager.Instance.GetManager<T>() ?? (!PluginManager.Instance.IsLoadingManager<T>()
                       ? (T)PluginManager.Instance.LoadManager(typeof(T))
                       : null);
        }

        public void Event(string name, Delegate @delegate) => Instance.AddEventHandler(name, @delegate);
        public PluginManager Instance => PluginManager.Instance;
        public Log Logger => Instance.Logger;

        protected Manager()
        {

        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}
