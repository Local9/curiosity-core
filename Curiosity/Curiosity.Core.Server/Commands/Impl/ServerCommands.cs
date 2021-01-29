using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Curiosity.Core.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Utils;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class ServerCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "srv" };
        public override string Title { get; set; } = "Server Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        #region Vehicles ONESYNC
        //[CommandInfo(new[] { "vehicle", "veh", "car" })]
        //public class VehicleSpawner : ICommand
        //{
        //    public async void On(CuriosityUser user, Player player, List<string> arguments)
        //    {
        //        try
        //        {
        //            if (arguments.Count <= 0) return;
        //            var model = API.GetHashKey(arguments.ElementAt(0));
        //            float x = arguments.ElementAt(1).ToFloat();
        //            float y = arguments.ElementAt(2).ToFloat();
        //            float z = arguments.ElementAt(3).ToFloat();

        //            Vector3 pos = new Vector3(x, y, z);
        //            int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
        //        }
        //        catch (Exception)
        //        {
        //            // Ignored
        //        }
        //    }
        //}
        #endregion

        #region WORLD
        [CommandInfo(new[] { "weather" })]
        public class WorldWeather : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnChatMessage(player, $"Missing weather type.");
                    return;
                }

                string arg = arguments.ElementAt(0);
                arg = arg.ToUpper();

                if (Enum.TryParse(arg, out WeatherType weather))
                {
                    WorldManager.WorldInstance.SetWeatherForAllRegions(weather);
                    ChatManager.OnChatMessage(player, $"Weather: {weather.GetStringValue()}");
                    return;
                }

                switch(arg)
                {
                    case "FREEZE":
                        WorldManager.WorldInstance.IsWeatherFrozen = !WorldManager.WorldInstance.IsWeatherFrozen;
                        ChatManager.OnChatMessage(player, WorldManager.WorldInstance.IsWeatherFrozen ? "Weather Frozen" : "Weather Unfrozen");
                        break;
                    default:
                        ChatManager.OnChatMessage(player, $"Argument '{arg}' unknown");
                        break;
                }
            }
        }
        [CommandInfo(new[] { "time" })]
        public class WorldTime : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnChatMessage(player, $"Missing arguments.");
                    return;
                }

                string arg1 = arguments.ElementAt(0);

                if (arguments.Count > 1)
                {
                    string arg2 = arguments.ElementAt(1);

                    if (int.TryParse(arg1, out int hour))
                    {
                        WorldManager.WorldInstance.ShiftTimeToHour(hour);
                    }

                    if (int.TryParse(arg2, out int minute))
                    {
                        WorldManager.WorldInstance.ShiftTimeToMinute(minute);
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
                        WorldManager.WorldInstance.IsTimeFrozen = !WorldManager.WorldInstance.IsTimeFrozen;
                        ChatManager.OnChatMessage(player, WorldManager.WorldInstance.IsTimeFrozen ? "Time Frozen" : "Time Unfrozen");
                        return;
                    default:
                        ChatManager.OnChatMessage(player, $"Argument '{arg1}' unknown.");
                        return;
                }
                ChatManager.OnChatMessage(player, $"Set Time: {arg1}");
                WorldManager.WorldInstance.ShiftTimeToHour(newHour);
                WorldManager.WorldInstance.ShiftTimeToMinute(newMinute);
            }
        }
        #endregion
    }
}
