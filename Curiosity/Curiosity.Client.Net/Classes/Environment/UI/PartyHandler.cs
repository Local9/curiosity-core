using CitizenFX.Core;
using System;
using System.Collections.Concurrent;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class PartyHandler
    {
        static Client client = Client.GetInstance();

        static ConcurrentDictionary<string, CitizenFX.Core.Player> PartyMembers = new ConcurrentDictionary<string, CitizenFX.Core.Player>();

        static public void Init()
        {
            // Invited to a party
            client.RegisterEventHandler("curiosity:client:party:member:invite", new Action<string>(OnPartyMemberInvite));
            // Left a party
            client.RegisterEventHandler("curiosity:client:party:member:left", new Action<string>(OnPartyMemberLeft));
            // Leaving a party
            client.RegisterEventHandler("curiosity:client:party:leave", new Action<string>(OnPartyLeave));
        }

        private static void OnPartyLeave(string obj)
        {
            BaseScript.TriggerServerEvent("curiosity:server:party:leave");
            PartyMembers.Clear();
        }

        private static void OnPartyMemberInvite(string encoded)
        {
            throw new NotImplementedException();
        }

        private static void OnPartyMemberLeft(string encoded)
        {
            throw new NotImplementedException();
        }
    }
}
