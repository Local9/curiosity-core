﻿using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("character:load", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                curiosityUser.Character = await Database.Store.CharacterDatabase.Get(curiosityUser.DiscordId);

                curiosityUser.Character.Skills = await Database.Store.SkillDatabase.Get(curiosityUser.Character.CharacterId);
                curiosityUser.Character.Stats = await Database.Store.StatDatabase.Get(curiosityUser.Character.CharacterId);

                return curiosityUser.Character;
            }));

            EventSystem.GetModule().Attach("character:save", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityCharacter curiosityCharacter = metadata.Find<CuriosityCharacter>(0);

                await curiosityCharacter.Save();

                return null;
            }));

            EventSystem.GetModule().Attach("character:get:profile", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                return PluginManager.ActiveUsers[metadata.Sender];
            }));

            EventSystem.GetModule().Attach("character:get:skills", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterSkill> returnVal = await Database.Store.SkillDatabase.Get(player.Character.CharacterId);
                player.Character.Skills = returnVal;
                return returnVal;
            }));

            EventSystem.GetModule().Attach("character:get:stats", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser player = PluginManager.ActiveUsers[metadata.Sender];
                List<CharacterStat> returnVal = await Database.Store.StatDatabase.Get(player.Character.CharacterId);
                player.Character.Stats = returnVal;
                return returnVal;
            }));


            Instance.ExportDictionary.Add("SkillAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, skillId, amt) => {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int newSkillValue = await Database.Store.SkillDatabase.Adjust(user.Character.CharacterId, skillId, amt);

                    user.Character.Skills = await Database.Store.SkillDatabase.Get(user.Character.CharacterId);

                    exportMessage.NewNumberValue = newSkillValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("StatAdjust", new Func<string, int, int, Task<string>>(
                async (playerHandle, statId, amt) => {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    int newSkillValue = await Database.Store.StatDatabase.Adjust(user.Character.CharacterId, statId, amt);

                    user.Character.Stats = await Database.Store.StatDatabase.Get(user.Character.CharacterId);

                    exportMessage.NewNumberValue = newSkillValue;

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("CashAdjust", new Func<string, int, Task<string>>(
                async (playerHandle, amt) => {

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
        }
    }
}
