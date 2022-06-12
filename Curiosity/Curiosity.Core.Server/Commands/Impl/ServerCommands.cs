﻿using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Managers;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class ServerCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "srv" };
        public override string Title { get; set; } = "Server Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        [CommandInfo(new[] { "giveMoney" })]
        public class GiveMoney : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    user.NotificationError($"Missing arguments. /srv giveMoney <playerId> <money>");
                    ChatManager.OnChatMessage(player, $"Missing arguments. /srv giveMoney <playerId> <money>");
                    return;
                }

                string arg = arguments.ElementAt(0);
                string cash = arguments.ElementAt(1);
                if (!int.TryParse(arg, out int playerId))
                {
                    user.NotificationError($"Player Argument is not a valid number.");
                    ChatManager.OnChatMessage(player, $"Player Argument is not a valid number.");
                    return;
                }

                if (!ulong.TryParse(cash, out ulong money))
                {
                    user.NotificationError($"Cash Argument is not a valid number.");
                    ChatManager.OnChatMessage(player, $"Cash Argument is not a valid number.");
                    return;
                }

                if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                {
                    user.NotificationError($"Player is missing from Active Users, did they disconnect?");
                    ChatManager.OnChatMessage(player, $"Player is missing from Active Users, did they disconnect?");
                    return;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[playerId];
                ulong originalValue = curiosityUser.Character.Cash;
                ulong newCashValue = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (long)money);
                curiosityUser.Character.Cash = newCashValue;

                DiscordClient.GetModule().SendDiscordPlayerLogMessage($"Player '{curiosityUser.LatestName}' cash adjust of '{money:N0}' (change '{originalValue:N0}' to '{newCashValue:N0}') by {user.LatestName}");

                ChatManager.OnChatMessage(player, $"Player '{curiosityUser.LatestName}' have been given ${money:N0}");

                user.NotificationSuccess($"Player '{curiosityUser.LatestName}' has been given ${money:N0}");
                curiosityUser.NotificationSuccess($"You have been given ${money:N0}");
            }
        }
        [CommandInfo(new[] { "removeMoney" })]
        public class RemoveMoney : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    user.NotificationError($"Missing arguments. /srv removeMoney <playerId> <money>");
                    ChatManager.OnChatMessage(player, $"Missing arguments. /srv removeMoney <playerId> <money>");
                    return;
                }

                string arg = arguments.ElementAt(0);
                string cash = arguments.ElementAt(1);
                if (!int.TryParse(arg, out int playerId))
                {
                    user.NotificationError($"Player Argument is not a valid number.");
                    ChatManager.OnChatMessage(player, $"Player Argument is not a valid number.");
                    return;
                }

                if (!ulong.TryParse(cash, out ulong money))
                {
                    user.NotificationError($"Cash Argument is not a valid number.");
                    ChatManager.OnChatMessage(player, $"Cash Argument is not a valid number.");
                    return;
                }

                if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                {
                    user.NotificationError($"Player is missing from Active Users, did they disconnect?");
                    ChatManager.OnChatMessage(player, $"Player is missing from Active Users, did they disconnect?");
                    return;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[playerId];
                ulong originalValue = curiosityUser.Character.Cash;
                ulong newCashValue = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (long)money * -1);
                curiosityUser.Character.Cash = newCashValue;

                DiscordClient.GetModule().SendDiscordPlayerLogMessage($"Player '{curiosityUser.LatestName}' cash adjust of '{money:N0}' (change '{originalValue:N0}' to '{newCashValue:N0}') by {user.LatestName}");

                ChatManager.OnChatMessage(player, $"Player '{curiosityUser.LatestName}' has had ${money:N0} removed.");

                user.NotificationSuccess($"Player '{curiosityUser.LatestName}' has had ${money:N0} removed.");
                curiosityUser.NotificationSuccess($"You have had ${money:N0} removed.");
            }
        }


        [CommandInfo(new[] { "wanted" })]
        public class PlayerWanted : ICommand
        {
            public void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnChatMessage(player, $"Missing argument.");
                    return;
                }

                string arg = arguments.ElementAt(0);
                string wanted = arguments.ElementAt(1);
                if (!int.TryParse(arg, out int playerId))
                {
                    ChatManager.OnChatMessage(player, $"Argument is not a valid number.");
                    return;
                }

                bool isWanted = wanted == "true";

                Player p = PluginManager.PlayersList[playerId];
                p.State.Set(StateBagKey.PLAYER_POLICE_WANTED, isWanted, true);
            }
        }

        #region WORLD
        [CommandInfo(new[] { "weather" })]
        public class WorldWeather : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnServerMessage(player, $"Missing weather type.");
                    return;
                }

                string arg = arguments.ElementAt(0);
                arg = arg.ToUpper();

                WorldManager world = WorldManager.GetModule();

                if (Enum.TryParse(arg, out WeatherType weather))
                {
                    world.SetWeatherForAllRegions(weather);
                    ChatManager.OnServerMessage(player, $"Weather: {weather}");
                    return;
                }

                switch (arg)
                {
                    case "FREEZE":
                        world.IsWeatherFrozen = !WorldManager.WorldInstance.IsWeatherFrozen;
                        ChatManager.OnServerMessage(player, WorldManager.WorldInstance.IsWeatherFrozen ? "Weather Frozen" : "Weather Unfrozen");
                        break;
                    default:
                        ChatManager.OnServerMessage(player, $"Argument '{arg}' unknown");
                        break;
                }
            }
        }

        [CommandInfo(new[] { "song" })]
        public class WorldSong : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnServerMessage(player, $"Missing url.");
                    return;
                }

                string url = arguments.ElementAt(0);
                float volume = arguments.Count == 2 ? float.Parse(arguments.ElementAt(1)) : 0.5f;

                PluginManager pluginManager = PluginManager.Instance;

                Vector3 pos = player.Character.Position;

                pluginManager.ExportDictionary["xsound"].PlayUrlPos(-1, "devSong", url, volume, pos, false);
            }
        }

        [CommandInfo(new[] { "tts" })]
        public class WorldTextToSpeech : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnServerMessage(player, $"Missing argument.");
                    return;
                }

                string text = arguments.ElementAt(0);
                float volume = arguments.Count == 2 ? float.Parse(arguments.ElementAt(1)) : 0.5f;

                PluginManager pluginManager = PluginManager.Instance;

                pluginManager.ExportDictionary["xsound"].TextToSpeech(-1, "devSpeak", "en-US", text, volume, false);
            }
        }
        #endregion
    }
}
