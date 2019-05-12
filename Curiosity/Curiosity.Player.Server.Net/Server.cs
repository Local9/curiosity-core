using CitizenFX.Core;
using System;

namespace Curiosity.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;

        public static Server GetInstance()
        {
            return _server;
        }

        public Server()
        {
            Debug.WriteLine("Entering Curiosity Server cter");

            _server = this;

            Debug.WriteLine("Leaving Curiosity Server cter");
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}
