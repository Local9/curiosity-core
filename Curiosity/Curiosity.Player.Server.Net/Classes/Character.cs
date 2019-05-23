using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes
{
    class Character
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Inventory:GetItems", new Action<Player>(GetActiveCharacterInventory));
        }

        static async void GetActiveCharacterInventory([FromSource]Player player)
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
