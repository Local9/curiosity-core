using Curiosity.Framework.Server.Events;
using Logger;
using System.Collections.Concurrent;

namespace Curiosity.Framework.Server.Managers
{
    public abstract class Manager<T> where T : Manager<T>, new()
    {
        public static T GetModule()
        {
            return null;
        }

        public PluginManager Instance { get; private set; }
        public PlayerList PlayerList => PluginManager.PlayerList;
        public ConcurrentDictionary<int, ClientId> UserSessions => PluginManager.UserSessions;

        public void Event(string name, Delegate @delegate) => Instance.AddEventHandler(name, @delegate);
        public ExportDictionary Export => Instance.ExportDictionary;
        public Log Logger => Instance.Logger;

        protected Manager()
        {
            Instance = PluginManager.Instance;
        }

        public virtual void Begin()
        {

        }
    }
}
