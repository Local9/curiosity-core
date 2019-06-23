using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Curiosity.Shared.Server.net.Helpers;
using Curiosity.Server.net.Business;
using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Server.net.Classes
{
    class Character
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Inventory:GetItems", new Action<CitizenFX.Core.Player>(GetActiveCharacterInventory));
            server.RegisterEventHandler("curiosity:Server:Character:RoleCheck", new Action<CitizenFX.Core.Player>(CharacterRoleCheck));
        }

        static async void CharacterRoleCheck([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                if (!SessionManager.SessionExists(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (session.IsStaff)
                {
                    return; // DON'T EDIT STAFF!
                }

                string discordIdentifier = player.Identifiers[Server.DISCORD_IDENTIFIER];

                if (string.IsNullOrEmpty(discordIdentifier))
                {
                    return;
                }

                Privilege privilege = await Discord.DiscordPrivilege(long.Parse(discordIdentifier), session.Privilege);
                session.Privilege = privilege;

                Database.DatabaseUsers.UpdateCharacterRole(session.User.CharacterId, privilege);
            }
            catch (Exception ex)
            {
                Log.Error($"GetActiveCharacterInventory -> {ex.Message}");
            }
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
