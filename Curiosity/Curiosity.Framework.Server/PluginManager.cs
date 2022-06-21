﻿using Curiosity.Framework.Server.Events;

namespace Curiosity.Framework.Server
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public ServerGateway Events;

        public PlayerList PlayerList;

        public PluginManager()
        {
            Logger.Trace($"CURIOSITY INITIATION");
            Instance = this;
            Events = new ServerGateway(Instance);

            Load();
        }

        public async void Load()
        {
            PlayerList = Players;

            bool databaseTest = await Database.DapperDatabase<bool>.GetSingleAsync("select 1;");
            if (databaseTest)
            {
                Logger.Trace($"Database Connection Test Successful!");
            }
            else
            {
                Logger.CriticalError($"Database Connection Test Failed!");
            }

            Logger.Trace($"CURIOSITY INITIATED");
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
