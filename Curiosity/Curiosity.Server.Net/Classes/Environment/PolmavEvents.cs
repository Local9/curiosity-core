using CitizenFX.Core;
using System;

namespace Curiosity.Server.net.Classes.Environment
{
    class PolmavEvents
    {
        static Server server = Server.GetInstance();

        private const string CLIENT_POLMAV_SPOTLIGHT_PAUSE = "curiosity:player:police:spotlight:pause";
        private const string CLIENT_POLMAV_SPOTLIGHT_MANUAL = "curiosity:player:police:spotlight:manual";
        private const string CLIENT_POLMAV_SPOTLIGHT_TOGGLE = "curiosity:player:police:spotlight:toggle";
        private const string CLIENT_POLMAV_SPOTLIGHT_FORWARD = "curiosity:player:police:spotlight:forward";
        private const string CLIENT_POLMAV_SPOTLIGHT_TRACKING = "curiosity:player:police:spotlight:tracking";
        private const string CLIENT_POLMAV_SPOTLIGHT_TRACKING_TOGGLE = "curiosity:player:police:spotlight:tracking:toggle";
        private const string CLIENT_POLMAV_SPOTLIGHT_BRIGHTNESS_UP = "curiosity:player:police:spotlight:light:up";
        private const string CLIENT_POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN = "curiosity:player:police:spotlight:light:down";
        private const string CLIENT_POLMAV_SPOTLIGHT_RADIUS_UP = "curiosity:player:police:spotlight:radius:up";
        private const string CLIENT_POLMAV_SPOTLIGHT_RADIUS_DOWN = "curiosity:player:police:spotlight:radius:down";

        private const string SERVER_POLMAV_SPOTLIGHT_PAUSE = "curiosity:server:police:spotlight:pause";
        private const string SERVER_POLMAV_SPOTLIGHT_MANUAL = "curiosity:server:police:spotlight:manual";
        private const string SERVER_POLMAV_SPOTLIGHT_TOGGLE = "curiosity:server:police:spotlight:toggle";
        private const string SERVER_POLMAV_SPOTLIGHT_FORWARD = "curiosity:server:police:spotlight:forward";
        private const string SERVER_POLMAV_SPOTLIGHT_TRACKING = "curiosity:server:police:spotlight:tracking";
        private const string SERVER_POLMAV_SPOTLIGHT_TRACKING_TOGGLE = "curiosity:server:police:spotlight:tracking:toggle";
        private const string SERVER_POLMAV_SPOTLIGHT_BRIGHTNESS_UP = "curiosity:server:police:spotlight:light:up";
        private const string SERVER_POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN = "curiosity:server:police:spotlight:light:down";
        private const string SERVER_POLMAV_SPOTLIGHT_RADIUS_UP = "curiosity:server:police:spotlight:radius:up";
        private const string SERVER_POLMAV_SPOTLIGHT_RADIUS_DOWN = "curiosity:server:police:spotlight:radius:down";

        static public void Init()
        {
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_PAUSE, new Action<CitizenFX.Core.Player, bool>(OnPauseTrackingSpotlight));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_MANUAL, new Action<CitizenFX.Core.Player>(OnManualSpotlight));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_TOGGLE, new Action<CitizenFX.Core.Player>(OnManualSpotlightToggle));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_FORWARD, new Action<CitizenFX.Core.Player, bool>(OnForwardSpotlight));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_TRACKING, new Action<CitizenFX.Core.Player, int, string, float, float, float>(OnTrackingSpotLight));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_TRACKING_TOGGLE, new Action<CitizenFX.Core.Player, bool>(OnTrackingSpotLightToggle));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_BRIGHTNESS_UP, new Action<CitizenFX.Core.Player>(OnSpotlightLightUp));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN, new Action<CitizenFX.Core.Player>(OnSpotlightLightDown));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_RADIUS_UP, new Action<CitizenFX.Core.Player>(OnSpotlightRadiusUp));
            server.RegisterEventHandler(SERVER_POLMAV_SPOTLIGHT_RADIUS_DOWN, new Action<CitizenFX.Core.Player>(OnSpotlightRadiusDown));
        }

        private static void OnSpotlightRadiusDown([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_RADIUS_DOWN);
        }

        private static void OnSpotlightRadiusUp([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_RADIUS_UP);
        }

        private static void OnSpotlightLightDown([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN);
        }

        private static void OnSpotlightLightUp([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_BRIGHTNESS_UP);
        }

        private static void OnTrackingSpotLightToggle([FromSource]CitizenFX.Core.Player player, bool state)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_TRACKING_TOGGLE, state);
        }

        private static void OnTrackingSpotLight([FromSource]CitizenFX.Core.Player player, int targetVehicleId, string targetLicensePlate, float targetPosX, float targetPosY, float targetPosZ)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_TRACKING, player.Handle, targetVehicleId, targetLicensePlate, targetPosX, targetPosY, targetPosZ);
        }

        private static void OnForwardSpotlight([FromSource]CitizenFX.Core.Player player, bool state)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_FORWARD, player.Handle, state);
        }

        private static void OnManualSpotlightToggle([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_TOGGLE);
        }

        private static void OnManualSpotlight([FromSource]CitizenFX.Core.Player player)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_MANUAL, player.Handle);
        }

        private static void OnPauseTrackingSpotlight([FromSource]CitizenFX.Core.Player player, bool state)
        {
            Server.TriggerClientEvent(CLIENT_POLMAV_SPOTLIGHT_PAUSE, state);
        }
    }
}
