using CitizenFX.Core;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;

namespace Curiosity.Brain.Server.net
{
    public class SessionManager : BaseScript
    {
        string currentlyHosting = null;
        List<Action> hostReleaseCallbacks = new List<Action>();

        public SessionManager()
        {
            RegisterEventHandler("hostingSession", new Action<Player>(HostingSession));
            RegisterEventHandler("hostedSession", new Action<Player>(HostedSession));

            NativeWrappers.EnableEnhancedHostSupport(true);
            Debug.WriteLine("SessionManager initialized");
        }

        public void HostingSession([FromSource] Player source)
        {
            if (!String.IsNullOrEmpty(currentlyHosting))
            {
                TriggerClientEvent(source, "sessionHostResult", "wait");
                hostReleaseCallbacks.Add(() => TriggerClientEvent(source, "sessionHostResult", "free"));
            }

            string hostId;
            // (If no host exists yet null exception is thrown)
            try
            {
                hostId = NativeWrappers.GetHostId();
            }
            catch (NullReferenceException)
            {
                hostId = null;
            }

            if (!String.IsNullOrEmpty(hostId) && NativeWrappers.GetPlayerLastMsg(hostId) < 1000)
            {
                TriggerClientEvent(source, "sessionHostResult", "conflict");
                return;
            }

            hostReleaseCallbacks.Clear();
            currentlyHosting = source.Handle;
            Log.Info($"[SESSION HOST] current game host is '{currentlyHosting}'");

            TriggerClientEvent(source, "sessionHostResult", "go");

            BaseScript.Delay(5000);
            currentlyHosting = null;
            hostReleaseCallbacks.ForEach(f => f());
        }

        public void HostedSession([FromSource] Player source)
        {
            if (currentlyHosting != source.Handle && !String.IsNullOrEmpty(currentlyHosting))
            {
                Log.Info($@"[SESSION HOST] Player client ""lying"" about being the host: current host '#{currentlyHosting}' != client '#{source.Handle}'");
                return;
            }

            hostReleaseCallbacks.ForEach(f => f());

            currentlyHosting = null;

            return;
        }

        public void RegisterEventHandler(string name, Delegate trigger)
        {
            EventHandlers[name] += trigger;
        }
    }
}
