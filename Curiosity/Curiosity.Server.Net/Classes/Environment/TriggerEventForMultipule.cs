using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
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
        }

        static void TriggerEventForAll([FromSource]CitizenFX.Core.Player player, string serializedModel)
        {
            try
            {
                var eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<TriggerEventForAll>(serializedModel);
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
