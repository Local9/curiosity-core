using Curiosity.Racing.Client.Events;

namespace Curiosity.Racing.Client.Managers
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
        public EventSystem EventSystem => EventSystem.GetModule();

        protected Manager()
        {
            Curiosity = CuriosityPlugin.Instance;
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}