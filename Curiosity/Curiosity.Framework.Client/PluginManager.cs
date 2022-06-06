﻿using Curiosity.Framework.Client.Events;

namespace Curiosity.Framework.Client
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public ClientGateway ClientGateway;

        public PluginManager()
        {
            Instance = this;
            ClientGateway = new ClientGateway(this);

            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStart(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnResourceStop(string obj)
        {
            throw new NotImplementedException();
        }

        public void Hook(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }
    }
}
