using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

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


            Instance.ExportDictionary.Add("SkillAdjust", new Func<string, int, int, string>(
                (playerHandle, skillId, amt) => {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    return $"{exportMessage}";
                }));


            Instance.ExportDictionary.Add("StatAdjust", new Func<string, int, int, string>(
                (playerHandle, skillId, amt) => {

                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.Error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.Error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    return $"{exportMessage}";
                }));
        }
    }
}
