using Atlas.Roleplay.Server.Events;

namespace Atlas.Roleplay.Server.Managers
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
        public EventSystem EventSystem { get; set; }

        protected Manager()
        {
            Atlas = AtlasPlugin.Instance;
            EventSystem = EventSystem.GetModule();
        }

        public virtual void Begin()
        {
            // Ignored
        }
    }
}