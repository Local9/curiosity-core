using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        DateTime lastSave = DateTime.Now;

        public override void Begin()
        {
            EventSystem.GetModule().Attach("character:routing:base", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                int currentBucket = API.GetPlayerRoutingBucket(player.Handle);

                if (currentBucket != 1)
                    API.SetPlayerRoutingBucket(player.Handle, (int)RoutingBucket.LOBBY);

                player.State.Set($"{StateBagKey.VEH_BOAT_NETWORK_ID}", -1, true);
                player.State.Set($"{StateBagKey.VEH_PLANE_NETWORK_ID}", -1, true);
                player.State.Set($"{StateBagKey.VEH_HELI_NETWORK_ID}", -1, true);
                player.State.Set($"{StateBagKey.VEH_TRAILER_NETWORK_ID}", -1, true);
                player.State.Set($"{StateBagKey.VEH_NETWORK_ID}", -1, true);
                player.State.Set($"{StateBagKey.PLAYER_ROUTING}", (int)RoutingBucket.LOBBY, true);
                player.State.Set($"{StateBagKey.PLAYER_HANDLE}", player.Handle, true);

                API.SetPlayerCullingRadius($"{metadata.Sender}", 5000.0f);

                PluginManager.ActiveUsers[metadata.Sender].RoutingBucket = RoutingBucket.LOBBY;

                CuriosityUser u = PluginManager.ActiveUsers[metadata.Sender];
                u.Character.LastPosition = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);

                // Logger.Debug($"{player.Name}:{API.GetPlayerRoutingBucket(player.Handle)}");

                return null;
            }));

            EventSystem.GetModule().Attach("character:load", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                curiosityUser.Character = await Database.Store.CharacterDatabase.Get(curiosityUser.DiscordId);

                return curiosityUser.Character;
            }));

            EventSystem.GetModule().Attach("character:weapons:equip", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                int playerPed = player.Character.Handle;

                uint weaponHash = metadata.Find<uint>(0);
                int ammoCount = metadata.Find<int>(1);
                bool isHidden = metadata.Find<bool>(2);
                bool forceInHand = metadata.Find<bool>(3);

                API.GiveWeaponToPed(playerPed, weaponHash, ammoCount, isHidden, forceInHand);

                return true;
            }));

            EventSystem.GetModule().Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

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

                curiosityCharacter.Cash = await Database.Store.BankDatabase.Get(curiosityCharacter.CharacterId);

                if (player.Character != null)
                {
                    Vector3 pos = player.Character.Position;

                    if (!pos.IsZero)
                        curiosityCharacter.LastPosition = new Position(pos.X, pos.Y, pos.Z, player.Character.Heading);

                    player.State.Set($"{StateBagKey.PLAYER_NAME}", player.Name, true);
                    player.State.Set($"{StateBagKey.SERVER_HANDLE}", player.Handle, true);
                    player.State.Set($"{StateBagKey.PLAYER_CASH}", curiosityCharacter.Cash, true);

                    if (!user.IsStaff)
                    {
                        bool godModeEnabled = API.GetPlayerInvincible(player.Handle);
                        if (godModeEnabled)
                            Web.DiscordClient.GetModule().SendDiscordServerEventLogMessage($"Player [{player.Handle}] '{player.Name}#{user.UserId}' has God Mode Enabled, Does job '{user.CurrentJob}' allow God Mode?");
                    }
                }

                await curiosityCharacter.Save();
                return true;
            }));

            EventSystem.GetModule().Attach("character:respawn", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.Send("character:respawn:hospital");

                return null;
            }));

            EventSystem.GetModule().Attach("character:respawn:charge", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                int costOfRespawn = curiosityUser.Character.RespawnCharge();

                if (curiosityUser.Character.Cash >= costOfRespawn)
                {
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, costOfRespawn * -1);
                    curiosityUser.Send("character:respawn:hospital");
                }

                return null;
            }));

            EventSystem.GetModule().Attach("character:killedSelf", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, Stat.STAT_KILLED_SELF, 1);

                return null;
            }));

            EventSystem.GetModule().Attach("character:death", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, Stat.STAT_DEATH, 1);

                return null;
            }));

            EventSystem.GetModule().Attach("character:get:profile", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                return PluginManager.ActiveUsers[metadata.Sender];
            }));

            EventSystem.GetModule().Attach("character:get:profile:enhanced", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Find<int>(0)))
                    return null;

                return PluginManager.ActiveUsers[metadata.Find<int>(0)];
            }));

            EventSystem.GetModule().Attach("character:get:skills", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterSkill> returnVal = await Database.Store.SkillDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("character:get:skills:enhanced", new AsyncEventCallback(async metadata =>
            {
                bool isSamePlayer = metadata.Sender == metadata.Find<int>(0);
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Find<int>(0)];

                if (!player.AllowPublicStats && !isSamePlayer) return null;

                List<CharacterSkill> returnVal = await Database.Store.SkillDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("character:get:stats", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterStat> returnVal = await Database.Store.StatDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("character:get:stats:enhanced", new AsyncEventCallback(async metadata =>
            {
                bool isSamePlayer = metadata.Sender == metadata.Find<int>(0);
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Find<int>(0)];

                if (!player.AllowPublicStats && !isSamePlayer) return null;

                List<CharacterStat> returnVal = await Database.Store.StatDatabase.Get(player.Character.CharacterId);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("character:update:stat:timed", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    Stat stat = (Stat)metadata.Find<int>(0);
                    double time = metadata.Find<double>(1);

                    int val = await Database.Store.StatDatabase.Adjust(curiosityUser.Character.CharacterId, stat, time);
                    return val;
                }
                catch (Exception ex)
                {
                    Logger.Error($"{ex}");
                    return 0;
                }
            }));

            EventSystem.GetModule().Attach("character:killed:self", new AsyncEventCallback(async metadata =>
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

            Instance.ExportDictionary.Add("Skill", new Func<string, int, Task<string>>(
                async (playerHandle, skillId) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    CharacterSkillExport skill = await Database.Store.SkillDatabase.GetSkill(user.Character.CharacterId, skillId);

                    if (skill == null)
                    {
                        exportMessage.Error = "Error; no rows returned.";
                    }
                    else
                    {
                        exportMessage.Skill = skill;
                    }

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("SkillAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, skillId, amt) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int newSkillValue = await Database.Store.SkillDatabase.Adjust(user.Character.CharacterId, skillId, amt);

                    exportMessage.NewNumberValue = newSkillValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Stat", new Func<string, int, Task<string>>(
                async (playerHandle, statId) =>
                {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int statValue = await Database.Store.StatDatabase.Get(user.Character.CharacterId, (Stat)statId);

                    exportMessage.Value = statValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("StatAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, statId, amt) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                    {
                        exportMessage.Error = "First parameter is not a number";
                        return $"{exportMessage}";
                    }

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                    {
                        exportMessage.Error = "Player was not found";
                        return $"{exportMessage}";
                    }

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    if (!Enum.TryParse($"{statId}", out Stat stat))
                    {
                        exportMessage.Error = "StatID failed parse";
                        return $"{exportMessage}";
                    }

                    int newSkillValue = await Database.Store.StatDatabase.Adjust(user.Character.CharacterId, stat, amt);

                    exportMessage.NewNumberValue = newSkillValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Cash", new Func<string, Task<string>>(
                async (playerHandle) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int cashValue = await Database.Store.BankDatabase.Get(user.Character.CharacterId);

                    user.Character.Cash = cashValue;

                    exportMessage.Value = cashValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("CashAdjust", new Func<string, int, Task<string>>(
                async (playerHandle, amt) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int newCashValue = await Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt);

                    user.Character.Cash = newCashValue;

                    exportMessage.NewNumberValue = newCashValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Item", new Func<string, int, Task<string>>(
                async (playerHandle, itemId) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    CuriosityShopItem item = await Database.Store.CharacterDatabase.GetItem(user.Character.CharacterId, itemId);

                    exportMessage.Item = item;

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
