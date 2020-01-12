using Curiosity.System.Server.Events;

namespace Curiosity.System.Server.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return CuriosityPlugin.Instance.GetManager<T>() ?? (!CuriosityPlugin.Instance.IsLoadingManager<T>()
                       ? (T)CuriosityPlugin.Instance.LoadManager(typeof(T))
                       : null);
        }

        public CuriosityPlugin Curiosity { get; set; }
        public EventSystem EventSystem { get; set; }

        protected Manager()
        {
            Curiosity = CuriosityPlugin.Instance;
            EventSystem = EventSystem.GetModule();
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}