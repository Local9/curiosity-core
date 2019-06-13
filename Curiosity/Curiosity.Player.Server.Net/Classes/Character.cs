using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Server.net.Classes
{
    class Character
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Inventory:GetItems", new Action<CitizenFX.Core.Player>(GetActiveCharacterInventory));
        }

        static async void GetActiveCharacterInventory([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                if (!SessionManager.SessionExists(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                List<Inventory> inventories = await Database.DatabaseCharacterInventory.GetInventoryAsync(session.User.CharacterId);

                player.TriggerEvent("curiosity:Client:Inventory:Items", JsonConvert.SerializeObject(inventories));
            }
            catch(Exception ex)
            {
                Log.Error($"GetActiveCharacterInventory -> {ex.Message}");
            }
        }
    }
}
