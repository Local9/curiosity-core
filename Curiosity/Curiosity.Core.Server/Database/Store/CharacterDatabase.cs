using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using GHMatti.Data.MySQL.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class CharacterDatabase
    {
        public static async Task<CuriosityCharacter> Get(ulong discordId)
        {
            int serverId = PluginManager.ServerId;
            int starterCash = API.GetConvarInt("starter_cash", 100);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@DiscordID", discordId },
                { "@ServerID", serverId }
            };

            string myQuery = "CALL spGetCharacter(@DiscordID, @ServerID);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                CuriosityCharacter curiosityCharacter = new CuriosityCharacter();

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    if (kv.ContainsKey("CharacterJSON") && kv["CharacterJSON"] != null)
                        curiosityCharacter = JsonConvert.DeserializeObject<CuriosityCharacter>($"{kv["CharacterJSON"]}");

                    curiosityCharacter.CharacterId = kv["characterId"].ToInt();
                    curiosityCharacter.MarkedAsRegistered = kv["IsRegistered"].ToInt() == 1;
                    curiosityCharacter.Cash = kv["Cash"].ToLong();

                    if (!curiosityCharacter.MarkedAsRegistered && curiosityCharacter.Cash == 0)
                        curiosityCharacter.Cash = starterCash;
                }

                return curiosityCharacter;
            }
        }

        internal async static Task<List<CharacterKit>> GetKits(int characterId, int itemCategoryId)
        {
            List<CharacterKit> characterKits = new List<CharacterKit>();
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterId", characterId },
                { "@ItemCategoryId", itemCategoryId },
            };

            string myQuery = "CALL selCharacterKits(@CharacterId, @ItemCategoryId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return characterKits;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    CharacterKit kit = new CharacterKit();
                    kit.ItemId = kv["ItemId"].ToInt();
                    kit.ItemCategoryId = kv["ItemCategoryId"].ToInt();
                    kit.NumberOwned = kv["NumberOwned"].ToInt();
                    kit.Amount = kv["HealingAmount"].ToInt();
                    characterKits.Add(kit);
                }

                return characterKits;
            }
        }

        public static async Task Save(CuriosityCharacter curiosityCharacter)
        {
            string characterJson = JsonConvert.SerializeObject(curiosityCharacter);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterId", curiosityCharacter.CharacterId },
                { "@IsRegistered", curiosityCharacter.MarkedAsRegistered },
                { "@CharacterJSON", characterJson }
            };

            string myQuery = "CALL upCharacter(@CharacterId, @IsRegistered, @CharacterJSON);";

            await MySqlDatabase.mySQL.Query(myQuery, myParams);
        }

        // Should write an extension
        public static async Task<List<CuriosityShopItem>> GetItems(int characterId)
        {
            ResultSet kvp = await GetCharacterItems(characterId);
            List<CuriosityShopItem> lst = new List<CuriosityShopItem>();

            if (kvp.Count == 0)
                return lst;

            foreach (Dictionary<string, object> kv in kvp)
            {
                CuriosityShopItem i = new CuriosityShopItem();

                i.ItemId = kv["ItemId"].ToInt();
                i.Label = $"{kv["Label"]}";
                i.Description = $"{kv["Description"]}";
                i.IsDroppable = kv["IsDroppable"].ToBoolean();
                i.IsUsable = kv["IsUsable"].ToBoolean();
                i.CanCarry = kv["CanCarry"].ToBoolean();
                i.MaximumAllowed = kv["MaximumAllowed"].ToInt();
                i.HashKey = $"{kv["HashKey"]}";
                i.ImageUri = $"{kv["ImageUri"]}";
                i.NumberOwned = kv["NumberOwned"].ToInt();
                i.CarringMaxed = kv["CarringMaxed"].ToBoolean();
                i.IsHealingItem = kv["IsHealingItem"].ToBoolean();
                i.HealingAmount = kv["HealingAmount"].ToInt();
                i.CategoryId = kv["ItemCategoryId"].ToInt();

                if (kv.ContainsKey("ShopItemId") && kv["ShopItemId"] is not null)
                {
                    i.ShopItemId = kv["ShopItemId"].ToInt();
                    i.BuyValue = kv["BuyValue"].ToInt();
                    i.BuyBackValue = kv["BuyBackValue"].ToInt();
                    i.IsStockManaged = kv["IsStockManaged"].ToBoolean();
                }

                lst.Add(i);
            }

            return lst;
        }

        // Should write an extension
        public static async Task<CuriosityShopItem> GetItem(int characterId, int itemId)
        {
            ResultSet kvp = await GetCharacterItems(characterId, itemId);

            if (kvp.Count == 0)
                return null;

            Dictionary<string, object> kv = kvp[0];

            CuriosityShopItem i = new CuriosityShopItem();

            i.ItemId = kv["ItemId"].ToInt();
            i.Label = $"{kv["Label"]}";
            i.Description = $"{kv["Description"]}";
            i.IsDroppable = kv["IsDroppable"].ToBoolean();
            i.IsUsable = kv["IsUsable"].ToBoolean();
            i.MaximumAllowed = kv["MaximumAllowed"].ToInt();
            i.HashKey = $"{kv["HashKey"]}";
            i.ImageUri = $"{kv["ImageUri"]}";
            i.NumberOwned = kv["NumberOwned"].ToInt();
            i.CanCarry = kv["CanCarry"].ToBoolean();
            i.CarringMaxed = kv["CarringMaxed"].ToBoolean();
            i.SpawnTypeId = (SpawnType)kv["SpawnTypeId"].ToInt();
            i.IsHealingItem = kv["IsHealingItem"].ToBoolean();
            i.HealingAmount = kv["HealingAmount"].ToInt();
            i.CategoryId = kv["ItemCategoryId"].ToInt();

            if (kv.ContainsKey("ShopItemId") && kv["ShopItemId"] is not null)
            {
                i.ShopItemId = kv["ShopItemId"].ToInt();
                i.BuyValue = kv["BuyValue"].ToInt();
                i.BuyBackValue = kv["BuyBackValue"].ToInt();
                i.IsStockManaged = kv["IsStockManaged"].ToBoolean();
            }

            return i;
        }

        // Should write an extension
        public static async Task<List<CuriosityShopItem>> GetInventoryEquipped(int characterId)
        {
            ResultSet kvp = await GetCharacterEquippedItems(characterId, inventoryOnly: true);
            List<CuriosityShopItem> lst = new List<CuriosityShopItem>();

            if (kvp.Count == 0)
                return lst;

            foreach (Dictionary<string, object> kv in kvp)
            {
                CuriosityShopItem i = new CuriosityShopItem();

                i.ItemId = kv["ItemId"].ToInt();
                i.Label = $"{kv["Label"]}";
                i.Description = $"{kv["Description"]}";
                i.IsDroppable = kv["IsDroppable"].ToBoolean();
                i.IsUsable = kv["IsUsable"].ToBoolean();
                i.MaximumAllowed = kv["MaximumAllowed"].ToInt();
                i.HashKey = $"{kv["HashKey"]}";
                i.ImageUri = $"{kv["ImageUri"]}";
                i.NumberOwned = kv["NumberOwned"].ToInt();
                i.CanCarry = kv["CanCarry"].ToBoolean();
                i.SpawnTypeId = (SpawnType)kv["SpawnTypeId"].ToInt();
                // i.CarringMaxed = kv["CarringMaxed"].ToBoolean();

                lst.Add(i);
            }

            return lst;
        }

        // Should write an extension
        public static async Task<List<CuriosityShopItem>> GetInventoryItems(int characterId)
        {
            ResultSet kvp = await GetCharacterItems(characterId, inventoryOnly: true);
            List<CuriosityShopItem> lst = new List<CuriosityShopItem>();

            if (kvp.Count == 0)
                return lst;

            foreach (Dictionary<string, object> kv in kvp)
            {
                CuriosityShopItem i = new CuriosityShopItem();

                i.ItemId = kv["ItemId"].ToInt();
                i.Label = $"{kv["Label"]}";
                i.Description = $"{kv["Description"]}";
                i.IsDroppable = kv["IsDroppable"].ToBoolean();
                i.IsUsable = kv["IsUsable"].ToBoolean();
                i.MaximumAllowed = kv["MaximumAllowed"].ToInt();
                i.HashKey = $"{kv["HashKey"]}";
                i.ImageUri = $"{kv["ImageUri"]}";
                i.NumberOwned = kv["NumberOwned"].ToInt();
                i.CanCarry = kv["CanCarry"].ToBoolean();
                i.CarringMaxed = kv["CarringMaxed"].ToBoolean();
                i.IsHealingItem = kv["IsHealingItem"].ToBoolean();
                i.HealingAmount = kv["HealingAmount"].ToInt();
                i.CategoryId = kv["ItemCategoryId"].ToInt();

                lst.Add(i);
            }

            return lst;
        }

        private static async Task<ResultSet> GetCharacterEquippedItems(int characterId, int? itemId = null, bool? inventoryOnly = null)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId },
                { "@InventoryOnly", inventoryOnly }
            };

            string myQuery = "CALL selCharacterEquippedItems(@CharacterID, @ItemID, @InventoryOnly);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                return await result;
            }
        }

        private static async Task<ResultSet> GetCharacterItems(int characterId, int? itemId = null, bool? inventoryOnly = null)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId },
                { "@InventoryOnly", inventoryOnly }
            };

            string myQuery = "CALL selCharacterItems(@CharacterID, @ItemID, @InventoryOnly);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                return await result;
            }
        }

        internal static async Task<bool> RemoveItem(int characterId, int itemId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId }
            };

            string myQuery = "CALL spCharacterRemoveItem(@CharacterID, @ItemID);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }

        internal static async Task<bool> InsertInventoryItem(int characterId, int itemId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId },
                { "@Amount", amount },
            };

            string myQuery = "CALL insCharacterInventoryItem(@CharacterID, @ItemID, @Amount);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }

        internal static async Task<bool> RemoveInventoryItem(int characterId, int itemId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId },
                { "@Amount", amount },
            };

            string myQuery = "CALL delCharacterInventoryItem(@CharacterID, @ItemID, @Amount);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }

        internal static async Task<bool> UseItem(int characterId, int itemId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@ItemID", itemId }
            };

            string myQuery = "CALL upCharacterIventoryItemUsed(@CharacterID, @ItemID);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }
    }
}
