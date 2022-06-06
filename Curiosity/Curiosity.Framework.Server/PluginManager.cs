using Curiosity.Framework.Server.Events;

namespace Curiosity.Framework.Server
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public ServerGateway Events;

        public PluginManager()
        {
            Instance = this;
            Events = new ServerGateway(Instance);
        }

        public void Hook(string eventName, Delegate @delegate)
        {
            Logger.Debug($"Registered Legacy Event Handler '{eventName}'");
            EventHandlers.Add(eventName, @delegate);
        }

        public static Player ToPlayer(int handle)
        {
            return Instance.Players[handle];
        }
    }
}
