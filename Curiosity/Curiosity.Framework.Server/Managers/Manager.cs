using Curiosity.Framework.Server.Events;
using Curiosity.Framework.Shared.Models;
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
        public ConcurrentDictionary<int, User> UserSessions => PluginManager.UserSessions;

        public void Event(string name, Delegate @delegate) => Instance.Hook(name, @delegate);
        public ExportDictionary Export => Instance.ExportDictionary;
        public ServerGateway ServerGateway => Instance.Events;

        protected Manager()
        {
            Instance = PluginManager.Instance;
        }

        public virtual void Begin()
        {

        }
    }
}
