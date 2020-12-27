namespace Curiosity.Systems.Library.EventWrapperLegacy
{
    public static class LegacyEvents
    {
        public static LegacyEvent<string, dynamic> Sync => new LegacyEvent<string, dynamic>("curiosity:sync");

        public static class Client
        {
            // LEGACY EVENTS
            public const string ReceivePlayerInformation = "curiosity:Client:Player:InternalInformation";
            public const string RequestPlayerInformation = "curiosity:Client:Player:Information";
            public const string ServerPlayerInformationUpdate = "curiosity:Client:Player:GetInformation";
            public const string PolicePatrolZone = "curiosity:Client:Mission:SetLocation";
            public const string PoliceDutyEvent = "curiosity:Client:Interface:Duty";
            public const string CurrentVehicle = "curiosity:Player:Menu:VehicleId";

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
                public const string PlayerSpawned = "playerSpawned";

                public static Event<string> OnClientResourceStart => new Event<string>("onClientResourceStart");
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
