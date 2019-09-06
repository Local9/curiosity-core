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
            server.RegisterEventHandler("curiosity:Server:Character:Save", new Action<CitizenFX.Core.Player, string>(CharacterSave));
        }
        static void CharacterSave([FromSource]CitizenFX.Core.Player player, string characterData)
        {
            try
            {
                if (!SessionManager.SessionExists(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                Database.DatabaseUsers.SaveCharacterSkins(session.User.CharacterId, characterData);

                Helpers.Notifications.Advanced($"Character Saved", $"", 20, player, Helpers.NotificationType.CHAR_LIFEINVADER);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "CharacterSave", $"{ex}");
                Log.Error($"GetActiveCharacterInventory -> {ex.Message}");
            }
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

                Privilege privilege = await Discord.DiscordPrivilege(long.Parse(discordIdentifier), session.Privilege, session.Player);

                if (session.Privilege == privilege) return;

                SessionManager.PlayerList[player.Handle].Privilege = privilege;
                SessionManager.PlayerList[player.Handle].User.RoleId = (int)privilege;

                Database.DatabaseUsers.UpdateCharacterRole(session.User.CharacterId, privilege);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "CharacterRoleCheck", $"{ex}");
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetActiveCharacterInventory", $"{ex}");
                Log.Error($"GetActiveCharacterInventory -> {ex.Message}");
            }
        }
    }
}
