using Curiosity.Interface.Client.Events;

namespace Curiosity.Interface.Client.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return CuriosityPlugin.Instance.GetManager<T>() ?? (!CuriosityPlugin.Instance.IsLoadingManager<T>()
                       ? (T)CuriosityPlugin.Instance.LoadManager(typeof(T))
                       : null);
        }

        public CuriosityPlugin Instance { get; set; }
        public EventSystem EventSystem => EventSystem.GetModule();

        protected Manager()
        {
            Instance = CuriosityPlugin.Instance;
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}