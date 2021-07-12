using CitizenFX.Core;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
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
                    ChatManager.OnServerMessage(player, $"Weather: {weather.GetStringValue()}");
                    return;
                }

                switch(arg)
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

                pluginManager.ExportDictionary["xsound"].TextToSpeech("tts", "en-US", text, volume, false);
            }
        }

        [CommandInfo(new[] { "time" })]
        public class WorldTime : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnServerMessage(player, $"Missing arguments.");
                    return;
                }

                string arg1 = arguments.ElementAt(0);

                WorldManager world = WorldManager.GetModule();

                if (arguments.Count > 1)
                {
                    string arg2 = arguments.ElementAt(1);

                    if (int.TryParse(arg1, out int hour))
                    {
                        world.ShiftTimeToHour(hour);
                    }

                    if (int.TryParse(arg2, out int minute))
                    {
                        world.ShiftTimeToMinute(minute);
                    }
                    return;
                }

                int newHour;
                int newMinute = 0;
                switch (arg1)
                {
                    case "morning":
                        newHour = 9;
                        
                        break;
                    case "noon":
                        newHour = 12;
                        break;
                    case "evening":
                        newHour = 18;
                        break;
                    case "night":
                        newHour = 22;
                        break;
                    case "freeze":
                        world.IsTimeFrozen = !world.IsTimeFrozen;
                        ChatManager.OnServerMessage(player, world.IsTimeFrozen ? "Time Frozen" : "Time Unfrozen");
                        return;
                    default:
                        ChatManager.OnServerMessage(player, $"Argument '{arg1}' unknown.");
                        return;
                }
                ChatManager.OnServerMessage(player, $"Set Time: {arg1}");
                world.ShiftTimeToHour(newHour);
                world.ShiftTimeToMinute(newMinute);
            }
        }
        #endregion
    }
}
