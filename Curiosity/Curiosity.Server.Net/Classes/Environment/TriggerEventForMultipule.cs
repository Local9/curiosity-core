using CitizenFX.Core;
using Curiosity.Global.Shared.Entity;
using Curiosity.Shared.Server.net.Helpers;
using System;

namespace Curiosity.Server.net.Classes.Environment
{
    static class TriggerEventForMultipule
    {
        static Server server = Server.GetInstance();

        static public void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Event:ForAll", new Action<CitizenFX.Core.Player, string>(TriggerEventForAll));
            // server.RegisterEventHandler("curiosity:Server:Event:ForAllPlayers", new Action<CitizenFX.Core.Player, string, object[]>(TriggerEventForAllPlayers));
        }

        static void TriggerEventForAllPlayers([FromSource]CitizenFX.Core.Player player, string eventName, params object[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 1:
                        Server.TriggerClientEvent(eventName, args[0]);
                        break;
                    case 2:
                        Server.TriggerClientEvent(eventName, args[0], args[1]);
                        break;
                    case 3:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2]);
                        break;
                    case 4:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2], args[3]);
                        break;
                    case 5:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2], args[3], args[4]);
                        break;
                    case 6:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2], args[3], args[4], args[5]);
                        break;
                    case 7:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                        break;
                    case 8:
                        Server.TriggerClientEvent(eventName, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                        break;
                    default:
                        Server.TriggerClientEvent(eventName);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[TriggerEventForAllPlayers] ERROR: {ex.Message}");
            }
        }

        static void TriggerEventForAll([FromSource]CitizenFX.Core.Player player, string serializedModel)
        {
            try
            {
                var eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<TriggerEventForAll>(serializedModel);

                // Log.Verbose($"Delete Vehicle: {eventData.EventName} {eventData.Payload}");

                if (eventData.passFullSerializedModel)
                {
                    eventData.PlayerServerId = Int32.Parse(player.Handle);
                    serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(eventData);
                    Server.TriggerClientEvent(eventData.EventName, serializedModel);
                }
                else
                {
                    Server.TriggerClientEvent(eventData.EventName, eventData.Payload);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[TriggerEventForAll] ERROR: {ex.Message}");
            }
        }
    }
}
