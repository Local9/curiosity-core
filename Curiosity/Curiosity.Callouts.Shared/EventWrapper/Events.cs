using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Shared.EventWrapper
{
    public static class Events
    {
        public static Event<string, dynamic> Sync => new Event<string, dynamic>("curiosity:sync");

        public static class Client
        {
            public static class Callout
            {
                public static Event<bool> EnableCalloutManager => new Event<bool>("curiosity:client:callout:manager:enable");
                public static Event<int> RequestFreeCops => new Event<int>("curiosity:client:callout:requestFreeCops");
                public static Event<string> ReceiveCallout => new Event<string>("curiosity:client:callout:receiveCallout");
                public static Event<int> Migration => new Event<int>("curiosity:client:callout:migration");
            }
        }

        public static class Native
        {
            public static class Client
            {
                public static Event<string> OnClientResourceStart => new Event<string>("onClientResourceStart");
                public static string PlayerSpawned => "playerSpawned";
            }

            public static class Server
            {
                public const string PlayerConnecting = "playerConnecting";
                public const string PlayerDropped = "playerDropped";
            }
        }

        public static class Server
        {
            public static class Callout
            {
                public static Event StartCallout => new Event("curiosity:server:callout:startCallout");
                public const string FreeCopsResponse = "curiosity:server:callout:freeCopsResponse";
                public const string CompleteCallout = "curiosity:server:callout:completed";
            }
        }
    }
}
