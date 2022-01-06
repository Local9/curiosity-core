using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        DateTime lastSave = DateTime.Now;
        const int MEDICAL_KIT_ITEM_ID = 495;
        const int JERRY_CAN_MAX = 4500;
        const int JERRY_CAN_START = 1485;

        public override void Begin()
        {
            EventSystem.Attach("character:routing:creator", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null) return null;

                int initRouting = 5000;
                int playerHandle = int.Parse(player.Handle);
                int routingId = initRouting + playerHandle;

                API.SetPlayerRoutingBucket(player.Handle, routingId);
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[player.Handle.ToInt()];
                curiosityUser.RoutingBucket = routingId;

                return null;
            }));

            EventSystem.Attach("character:revive:other", new AsyncEventCallback(async metadata =>
            {
                int otherId = metadata.Find<int>(0);
                ExportMessage exportMessage = new ExportMessage();


                Player sender = PluginManager.PlayersList[metadata.Sender];
                Player other = PluginManager.PlayersList[otherId];

                if (sender is null) goto USER_MISSING;
                if (other is null) goto USER_MISSING;

                CuriosityUser curiosityUserSender = PluginManager.ActiveUsers[metadata.Sender];
                CuriosityUser curiosityUserOther = PluginManager.ActiveUsers[otherId];

                if (curiosityUserSender is null) goto USER_MISSING;
                if (curiosityUserOther is null) goto USER_MISSING;

                CuriosityShopItem curiosityItem = await Database.Store.CharacterDatabase.GetItem(curiosityUserSender.Character.CharacterId, MEDICAL_KIT_ITEM_ID);

                if (curiosityItem is null)
                {
                    exportMessage.error = "You currently have no medical kits.";
                    goto SENDBACK;
                }

                if (curiosityItem.NumberOwned == 0)
                {
                    exportMessage.error = "You currently have no medical kits.";
                    goto SENDBACK;
                }

                bool success = await Database.Store.CharacterDatabase.UseItem(curiosityUserSender.Character.CharacterId, MEDICAL_KIT_ITEM_ID);

                if (!success)
                {
                    exportMessage.error = "Issue when updating items.";
                    goto SENDBACK;
                }

                curiosityUserOther.Send("character:respawnNow");
                goto SENDBACK;

            USER_MISSING:
                exportMessage.error = "User not found";

            SENDBACK:
                return exportMessage;
            }));

            EventSystem.Attach("character:inventory:items:all", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                List<CharacterItem> items = await Database.Store.CharacterDatabase.GetAllItems(curiosityUser.Character.CharacterId);

                return items;
            }));

            EventSystem.Attach("character:inventory:armor", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                List<CharacterKit> kits = await Database.Store.CharacterDatabase.GetKits(curiosityUser.Character.CharacterId, 21);

                return kits;
            }));

            EventSystem.Attach("character:inventory:health", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                List<CharacterKit> kits = await Database.Store.CharacterDatabase.GetKits(curiosityUser.Character.CharacterId, 19);

                return kits;
            }));

            EventSystem.Attach("character:inventory:repair", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                List<CharacterKit> kits = await Database.Store.CharacterDatabase.GetKits(curiosityUser.Character.CharacterId, 24);

                return kits;
            }));

            EventSystem.Attach("character:routing:base", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser u = PluginManager.ActiveUsers[metadata.Sender];

                int currentBucket = API.GetPlayerRoutingBucket(player.Handle);

                if (currentBucket != 0)
                {
                    API.SetPlayerRoutingBucket(player.Handle, 0);
                    u.RoutingBucket = 0;
                }

                player.State.Set(StateBagKey.VEH_BOAT_NETWORK_ID, -1, true);
                player.State.Set(StateBagKey.VEH_PLANE_NETWORK_ID, -1, true);
                player.State.Set(StateBagKey.VEH_HELI_NETWORK_ID, -1, true);
                player.State.Set(StateBagKey.VEH_TRAILER_NETWORK_ID, -1, true);
                player.State.Set(StateBagKey.VEH_NETWORK_ID, -1, true);

                player.State.Set(StateBagKey.PLAYER_ROUTING, 0, true);
                player.State.Set(StateBagKey.PLAYER_HANDLE, player.Handle, true);

                player.State.Set(StateBagKey.PLAYER_PASSIVE, u.Character.IsPassive, true);

                player.State.Set(StateBagKey.PLAYER_ROLE, (int)u.Role, true);
                player.State.Set(StateBagKey.PLAYER_JOB, (int)ePlayerJobs.UNEMPLOYED, true);
                player.State.Set(StateBagKey.PLAYER_IS_WANTED, false, true);
                player.State.Set(StateBagKey.PLAYER_WANTED_LEVEL, 0, true);

                SetEntityDistanceCullingRadius(player.Character.Handle, 0f); // default culling range

                u.RoutingBucket = 0;

                if (u.Character.LastPosition is null)
                    u.Character.LastPosition = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);

                List<CuriosityShopItem> lst = await Database.Store.CharacterDatabase.GetInventoryEquipped(u.Character.CharacterId);

                await BaseScript.Delay(100);

                // player.Character.Position = u.Character.LastPosition.AsVector();

                try
                {
                    while (!API.DoesEntityExist(player.Character.Handle))
                    {
                        await BaseScript.Delay(0);
                    }

                    for (int i = 0; i < lst.Count; i++)
                    {
                        CuriosityShopItem item = lst[i];

                        if (item.SpawnTypeId == SpawnType.Weapon)
                        {
                            int hash = API.GetHashKey(item.HashKey);
                            bool isJerryCan = item.HashKey == "weapon_petrolcan";
                            await BaseScript.Delay(0);
                            API.GiveWeaponToPed(player.Character.Handle, (uint)hash, isJerryCan ? JERRY_CAN_START : 999, false, false);
                        }
                    }

                    API.SetPlayerInvincible(player.Handle, false);
                }
                catch (Exception ex)
                {

                }

                // Logger.Debug($"{player.Name}:{API.GetPlayerRoutingBucket(player.Handle)}");

                return null;
            }));

            EventSystem.Attach("character:load", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                curiosityUser.Character = await Database.Store.CharacterDatabase.Get(curiosityUser.DiscordId);

                if (!curiosityUser.Character.MarkedAsRegistered)
                    curiosityUser.Character.IsPassive = true;

                return curiosityUser.Character;
            }));

            EventSystem.Attach("character:weapons:equip", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                int playerPed = player.Character.Handle;

                if (!API.DoesEntityExist(playerPed))
                {
                    return false;
                }

                uint weaponHash = metadata.Find<uint>(0);
                int ammoCount = metadata.Find<int>(1);
                bool isHidden = metadata.Find<bool>(2);
                bool forceInHand = metadata.Find<bool>(3);

                API.GiveWeaponToPed(playerPed, weaponHash, ammoCount, isHidden, forceInHand);

                return true;
            }));

            EventSystem.Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null)
                {
                    Logger.Debug($"Player doesn't exist, cannot save.");
                    return false;
                }

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    Logger.Debug($"Player doesn't exist, cannot save.");
                    return false;
                }

                CuriosityUser user = PluginManager.ActiveUsers[metadata.Sender];
                CuriosityCharacter curiosityCharacter = metadata.Find<CuriosityCharacter>(0);

                if (user.Character.CharacterId != curiosityCharacter.CharacterId)
                {
                    Logger.Debug($"Player CharacterId incorrect match.");
                    return false;
                }

                user.Character = curiosityCharacter;

                if (player.Character != null)
                {
                    Vector3 pos = player.Character.Position;

                    if (!pos.IsZero)
                        curiosityCharacter.LastPosition = new Position(pos.X, pos.Y, pos.Z, player.Character.Heading);

                    player.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                    player.State.Set(StateBagKey.SERVER_HANDLE, player.Handle, true);

                    if (!user.IsStaff)
                    {
                        bool godModeEnabled = API.GetPlayerInvincible(player.Handle);
                        if (godModeEnabled)
                        {
                            Web.DiscordClient.GetModule().SendDiscordServerEventLogMessage($"Player [{player.Handle}] '{player.Name}#{user.UserId}' has God Mode Enabled, Does job '{user.CurrentJob}' allow God Mode?");
                            await BaseScript.Delay(0);
                            if (user.CurrentJob != "FireFighter")
                            {
                                API.SetPlayerInvincible(player.Handle, false);
                                Web.DiscordClient.GetModule().SendDiscordServerEventLogMessage($"Player [{player.Handle}] '{player.Name}#{user.UserId}' God Mode removed as the job is not FireFighter.");
                                await BaseScript.Delay(0);
                            }
                        }
                    }
                }

                await curiosityCharacter.Save();
                return true;
            }));

            EventSystem.Attach("character:respawn", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.Send("character:respawn:hospital");

                return null;
            }));

            EventSystem.Attach("character:respawn:charge", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                int costOfRespawn = curiosityUser.Character.RespawnCharge();

                long totalSum = curiosityUser.Character.Cash - costOfRespawn;
                if (totalSum < 0)
                    costOfRespawn = 0;

                if (curiosityUser.Character.Cash >= costOfRespawn)
                {
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, costOfRespawn * -1);
                    curiosityUser.Send("character:respawn:hospital");
                }

                return null;
            }));

            EventSystem.Attach("character:killedSelf", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, Stat.STAT_KILLED_SELF, 1);

                return null;
            }));

            EventSystem.Attach("character:death", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, Stat.STAT_DEATH, 1);

                return null;
            }));

            EventSystem.Attach("character:get:profile", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                return PluginManager.ActiveUsers[metadata.Sender];
            }));

            EventSystem.Attach("character:get:profile:enhanced", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Find<int>(0)))
                    return null;

                return PluginManager.ActiveUsers[metadata.Find<int>(0)];
            }));

            EventSystem.Attach("character:get:skills", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterSkill> returnVal = await Database.Store.SkillDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.Attach("character:get:skills:enhanced", new AsyncEventCallback(async metadata =>
            {
                bool isSamePlayer = metadata.Sender == metadata.Find<int>(0);
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Find<int>(0)];

                if (!player.AllowPublicStats && !isSamePlayer) return null;

                List<CharacterSkill> returnVal = await Database.Store.SkillDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.Attach("character:get:stats", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterStat> returnVal = await Database.Store.StatDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.Attach("character:get:stats:enhanced", new AsyncEventCallback(async metadata =>
            {
                bool isSamePlayer = metadata.Sender == metadata.Find<int>(0);
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Find<int>(0)];

                if (!player.AllowPublicStats && !isSamePlayer) return null;

                List<CharacterStat> returnVal = await Database.Store.StatDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.Attach("character:update:stat:timed", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return 0;

                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    Stat stat = (Stat)metadata.Find<int>(0);
                    double time = metadata.Find<double>(1);

                    int val = await Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, stat, time);
                    return val;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Update Player Stat");
                    return 0;
                }
            }));

            EventSystem.Attach("character:killed:self", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];

                long costForDeath = 500 * player.TimesKilledSelf;

                player.TimesKilledSelf++;

                if (costForDeath > 10000)
                    costForDeath = 10000;

                if (player.Character.Cash - costForDeath < 0)
                    costForDeath = player.Character.Cash;

                if (costForDeath > 0)
                {
                    player.Character.Cash = await Database.Store.BankDatabase.Adjust(player.Character.CharacterId, costForDeath * -1);
                    await Database.Store.StatDatabase.Adjust(player.Character.CharacterId, Stat.STAT_KILLED_SELF, 1);
                }

                return player.Character.Cash;
            }));

            EventSystem.Attach("character:inventory:items", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];

                List<CuriosityShopItem> lst = await Database.Store.CharacterDatabase.GetInventoryItems(player.Character.CharacterId);

                return lst;
            }));

            EventSystem.Attach("character:inventory:hasItem", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];

                int itemId = metadata.Find<int>(0);

                CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(player.Character.CharacterId, itemId);

                if (item is null)
                    return false;

                return item.NumberOwned > 0;
            }));

            EventSystem.Attach("character:inventory:equipped", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];

                List<CuriosityShopItem> lst = await Database.Store.CharacterDatabase.GetInventoryEquipped(player.Character.CharacterId);

                return lst;
            }));

            EventSystem.Attach("character:inventory:success", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                int itemId = metadata.Find<int>(0);
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                await Database.Store.CharacterDatabase.UseItem(curiosityUser.Character.CharacterId, itemId);
                await BaseScript.Delay(0);
                return null;
            }));

            EventSystem.Attach("character:inventory:use", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                ExportMessage exportMessage = new ExportMessage();

                int itemId = metadata.Find<int>(0);
                int networkId = metadata.Find<int>(1);

                CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(curiosityUser.Character.CharacterId, itemId);
                await BaseScript.Delay(0);
                if (!item.IsUsable)
                {
                    exportMessage.error = "Item is not usable.";
                    goto ReturnResult;
                }

                if (item.NumberOwned == 0)
                {
                    exportMessage.error = "You do not have any left.";
                    goto ReturnResult;
                }

                int entityHandle = NetworkGetEntityFromNetworkId(networkId);

                if (entityHandle > 0)
                {
                    int entType = GetEntityType(entityHandle);
                    if (entType == 1)
                    {
                        Ped ped = new Ped(entityHandle);
                        if (item.CategoryId == 19)
                        {
                            int health = GetEntityHealth(entityHandle);
                        }

                        if (item.CategoryId == 21)
                        {
                            SetPedArmour(entityHandle, item.HealingAmount);
                        }
                    }

                    if (entType == 2)
                    {
                        Vehicle vehicle = new Vehicle(entityHandle);
                        SetVehicleBodyHealth(entityHandle, 1000f);
                    }
                }

                exportMessage.item = item;
            ReturnResult:
                return exportMessage;
            }));

            EventSystem.Attach("character:inventory:equip", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[metadata.Sender];

                ExportMessage exportMessage = new ExportMessage();

                int itemId = metadata.Find<int>(0);
                int amount = metadata.Find<int>(1);

                CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(curiosityUser.Character.CharacterId, itemId);

                if (item.CarringMaxed)
                {
                    exportMessage.error = "Carrying Maximum Allowed.";
                    goto ReturnResult;
                }

                bool inserted = await Database.Store.CharacterDatabase.InsertInventoryItem(curiosityUser.Character.CharacterId, itemId, amount);

                if (!inserted)
                {
                    exportMessage.error = "Was unable to equip the item.";
                    goto ReturnResult;
                }

                if (item.SpawnTypeId == SpawnType.Weapon)
                {
                    int hash = API.GetHashKey(item.HashKey);
                    bool isJerryCan = item.HashKey == "weapon_petrolcan";
                    await BaseScript.Delay(0);
                    API.GiveWeaponToPed(player.Character.Handle, (uint)hash, isJerryCan ? 10 : 999, false, false);

                    Logger.Debug($"Equipping {item.HashKey}:{hash} to {curiosityUser.LatestName}");
                }

            ReturnResult:
                return exportMessage;
            }));

            EventSystem.Attach("character:inventory:remove", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[metadata.Sender];

                ExportMessage exportMessage = new ExportMessage();

                int itemId = metadata.Find<int>(0);
                int amount = metadata.Find<int>(1);

                CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(curiosityUser.Character.CharacterId, itemId);

                bool updated = await Database.Store.CharacterDatabase.RemoveInventoryItem(curiosityUser.Character.CharacterId, itemId, amount);

                if (!updated)
                {
                    exportMessage.error = "Was unable to remove the item.";
                    goto ReturnResult;
                }

                if (item.SpawnTypeId == SpawnType.Weapon)
                {
                    int hash = API.GetHashKey(item.HashKey);
                    await BaseScript.Delay(0);
                    API.RemoveWeaponFromPed(player.Character.Handle, (uint)hash);

                    Logger.Debug($"Removing {item.HashKey}:{hash} from {curiosityUser.LatestName}");
                }

            ReturnResult:
                return exportMessage;
            }));


            /// EXPORTS

            Instance.ExportDictionary.Add("Skill", new Func<string, int, Task<string>>(
                async (playerHandle, skillId) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }


                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    CharacterSkillExport skill = await Database.Store.SkillDatabase.GetSkill(user.Character.CharacterId, skillId);

                    if (skill == null)
                    {
                        exportMessage.error = "Error; no rows returned.";
                    }
                    else
                    {
                        exportMessage.skill = skill;
                    }

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("SkillAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, skillId, amt) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int newSkillValue = await Database.Store.SkillDatabase.Adjust(user.Character.CharacterId, skillId, amt);

                    exportMessage.newNumberValue = newSkillValue;

                    DiscordClient.GetModule().SendDiscordPlayerLogMessage($"Player '{user.LatestName}' skill '{skillId}' changed by '{amt}' (new value: {newSkillValue})");
                    await BaseScript.Delay(0);

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Stat", new Func<string, int, Task<string>>(
                async (playerHandle, statId) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int statValue = await Database.Store.StatDatabase.Get(user.Character.CharacterId, (Stat)statId);

                    exportMessage.value = statValue;

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("StatAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, statId, amt) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    if (!Enum.TryParse($"{statId}", out Stat stat))
                    {
                        exportMessage.error = "StatID failed parse";
                        goto SendMessage;
                    }

                    int newSkillValue = await Database.Store.StatDatabase.Adjust(user.Character.CharacterId, stat, amt);

                    DiscordClient.GetModule().SendDiscordPlayerLogMessage($"Player '{user.LatestName}' stat '{stat}' changed by '{amt}' (new value: {newSkillValue})");
                    await BaseScript.Delay(0);

                    exportMessage.newNumberValue = newSkillValue;

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Cash", new Func<string, Task<string>>(
                async (playerHandle) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int cashValue = await Database.Store.BankDatabase.Get(user.Character.CharacterId);

                    user.Character.Cash = cashValue;

                    exportMessage.value = cashValue;

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("CashAdjust", new Func<string, int, Task<string>>(
                async (playerHandle, amt) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    long originalValue = user.Character.Cash;

                    int newCashValue = await Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt);

                    user.Character.Cash = newCashValue;

                    exportMessage.newNumberValue = newCashValue;

                    DiscordClient.GetModule().SendDiscordPlayerLogMessage($"Player '{user.LatestName}' cash adjust of '{amt}' (change '{originalValue}' to '{newCashValue}')");
                    await BaseScript.Delay(0);

                SendMessage:
                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Item", new Func<string, int, Task<string>>(
                async (playerHandle, itemId) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.error = "First parameter is not a number";
                        goto SendMessage;
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.error = "Player was not found";
                        goto SendMessage;
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(user.Character.CharacterId, itemId);

                    exportMessage.item = item;

                SendMessage:
                    return $"{exportMessage}";
                }));
        }

        public float ExperienceModifier(Role role)
        {
            switch (role)
            {
                case Role.DONATOR_LEVEL_1:
                    return float.Parse(API.GetConvar("experience_modifier_donator1", $"0.1"));
                case Role.DONATOR_LEVEL_2:
                    return float.Parse(API.GetConvar("experience_modifier_donator2", $"0.25"));
                case Role.DONATOR_LEVEL_3:
                    return float.Parse(API.GetConvar("experience_modifier_donator3", $"0.5"));
                default:
                    return float.Parse(API.GetConvar("experience_modifier_lifeTime", $"0.05"));
            }
        }

        //[TickHandler]
        //private async void OnSaveCharacters()
        //{
        //    if (DateTime.Now.Subtract(lastSave).TotalMinutes >= 5)
        //    {

        //        foreach (KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
        //        {
        //            CuriosityUser curiosityUser = kvp.Value;
        //            curiosityUser.Character.Cash = await Database.Store.BankDatabase.Get(curiosityUser.Character.CharacterId);
        //            Player player = PluginManager.PlayersList[kvp.Key];

        //            if (player.Character != null)
        //            {
        //                Vector3 pos = player.Character.Position;

        //                if (!pos.IsZero)
        //                    curiosityUser.Character.LastPosition = new Position(pos.X, pos.Y, pos.Z, player.Character.Heading);

        //                player.State.Set($"{StateBagKey.PLAYER_NAME}", player.Name, true);
        //                player.State.Set($"{StateBagKey.SERVER_HANDLE}", player.Handle, true);
        //                player.State.Set($"{StateBagKey.PLAYER_CASH}", curiosityUser.Character.Cash, true);

        //                int pedHandle = player.Character.Handle;

        //                curiosityUser.Character.Armor = API.GetPedArmour(pedHandle);
        //                curiosityUser.Character.Health = API.GetEntityHealth(pedHandle);
        //                curiosityUser.Character.IsDead = curiosityUser.Character.Health == 0;

        //                if (!curiosityUser.IsStaff)
        //                {
        //                    bool godModeEnabled = API.GetPlayerInvincible(player.Handle);
        //                    if (godModeEnabled)
        //                        Web.DiscordClient.DiscordInstance.SendDiscordServerEventLogMessage($"Player {player.Name} has God Mode Enabled, Does job '{curiosityUser.CurrentJob}' allow God Mode?");
        //                }
        //            }

        //            await curiosityUser.Character.Save();
        //        }

        //        lastSave = DateTime.Now;
        //    }
        //}
    }
}
