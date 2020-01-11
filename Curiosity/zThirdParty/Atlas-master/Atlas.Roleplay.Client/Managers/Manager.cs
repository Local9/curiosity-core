using Atlas.Roleplay.Client.Events;

namespace Atlas.Roleplay.Client.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return AtlasPlugin.Instance.GetManager<T>() ?? (!AtlasPlugin.Instance.IsLoadingManager<T>()
                       ? (T) AtlasPlugin.Instance.LoadManager(typeof(T))
                       : null);
        }

        public AtlasPlugin Atlas { get; set; }
        public EventSystem EventSystem => EventSystem.GetModule();

        protected Manager()
        {
            Atlas = AtlasPlugin.Instance;
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}