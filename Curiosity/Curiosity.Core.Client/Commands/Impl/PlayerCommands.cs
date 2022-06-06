using CitizenFX.Core;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.Core.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        static EventSystem EventSystem => EventSystem.GetModule();

        public override string[] Aliases { get; set; } = { "player", "p", "me" };
        public override string Title { get; set; } = "Player Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; }
        public override List<Role> RequiredRoles { get; set; }

        [CommandInfo(new[] { "weather", })]
        public class WeatherForecast : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                WorldManager.GetModule().ShowWeatherForecast();
            }
        }

        [CommandInfo(new[] { "unstuck", })]
        public class Unstuck : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {

                bool isWantedByPolice = Game.Player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

                if (Game.Player.WantedLevel > 0)
                {
                    Notify.Error($"Cannot spawn/change a vehicle when wanted.");
                    return;
                }

                if (isWantedByPolice)
                {
                    Notify.Error($"Cannot spawn/change a vehicle when wanted.");
                    return;
                }

                Position position = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);
                EventSystem.Send("world:routing:city");
                await SafeTeleport.TeleportFadePlayer(position);
            }
        }

        [CommandInfo(new[] { "sound", })]
        public class XSound : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                PluginManager pluginManager = PluginManager.Instance;
                var xsound = pluginManager.ExportDictionary["xsound"] ?? null;
                if (pluginManager.ExportDictionary["xsound"] is null)
                {
                    Notify.Info($"XSound was not found");
                    return;
                }

                if (arguments.Count == 0)
                {
                    Chat.SendLocalMessage("Args missing; all, stop, link", "help");
                }

                string cmd = arguments.ElementAt(0);

                if (!xsound.isPlayerCloseToAnySound())
                {
                    Chat.SendLocalMessage($"You're currently not near any sounds.", "help");
                    return;
                }

                if (cmd == "all")
                {
                    var allSounds = xsound.getAllAudioInfo();

                    string json = JsonConvert.SerializeObject(allSounds);

                    Dictionary<string, dynamic> sounds = new Dictionary<string, dynamic>();

                    sounds = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

                    List<string> soundsPlaying = new();

                    foreach (KeyValuePair<string, dynamic> keyValuePair in sounds)
                    {
                        soundsPlaying.Add(keyValuePair.Key);
                    }

                    Chat.SendLocalMessage($"Sounds: {string.Join(", ", soundsPlaying)}", "help");
                    return;
                }

                if (cmd == "stop" || cmd == "link")
                {
                    string soundName = arguments.ElementAt(1);

                    if (!xsound.soundExists(soundName))
                    {
                        Chat.SendLocalMessage($"'{soundName}' not found", "help");
                        return;
                    }

                    if (cmd == "stop")
                    {
                        xsound.Destroy(soundName);
                        Chat.SendLocalMessage($"'{soundName}' Stopped", "help");
                        return;
                    }

                    if (cmd == "link")
                    {
                        string url = xsound.getLink(soundName);
                        Chat.SendLocalMessage($"'{soundName}' URL: {url}", "help");
                        return;
                    }
                }

                Chat.SendLocalMessage($"Command '{cmd}' unknown.", "help");
            }
        }

        [CommandInfo(new[] { "tow" })]
        public class PlayerTow : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = Cache.PlayerPed.GetVehicleInFront();
                NotificationManager notificationManger = NotificationManager.GetModule();

                if (vehicle == null)
                {
                    notificationManger.Info($"<b>Invalid Information</b><br />Sorry bud, cannot find ere on this ere computer that there vehicle.");
                    return;
                }

                int vehicleDeletionSent = await EventSystem.Request<int>("vehicle:tow", vehicle.NetworkId);
                CommonErrors commonErrors = (CommonErrors)vehicleDeletionSent;

                switch (commonErrors)
                {
                    case CommonErrors.PurchaseSuccessful:
                        notificationManger.Success($"<b>Vehicle Impounded</b><br /><b>Charge</b>: $1000<br />Pleasure doing business with ye.");
                        await vehicle.FadeOut();
                        vehicle.Delete();
                        break;
                    case CommonErrors.PurchaseUnSuccessful:
                        notificationManger.Info($"<b>Payment Issue</b><br />Looks like ur bank rejected it.");
                        break;
                    case CommonErrors.VehicleIsOwned:
                        notificationManger.Warn($"<b>Computer Warning</b><br />Sorry bub, this vehicle is owned by someone.");
                        break;
                    default:
                        notificationManger.Error($"Computer Error", "Computer said no...");
                        break;
                }
            }
        }

        [CommandInfo(new[] { "flip" })]
        public class PlayerFlip : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = Cache.PlayerPed.GetVehicleInFront();
                NotificationManager notificationManger = NotificationManager.GetModule();

                if (vehicle == null)
                {
                    notificationManger.Info($"Vehicle not found.");
                    return;
                }

                if (!vehicle.IsOnAllWheels)
                {
                    await vehicle.FadeOut();

                    vehicle.IsCollisionEnabled = false;

                    vehicle.Rotation = new Vector3(vehicle.Rotation.X, 0f, vehicle.Rotation.Z);

                    vehicle.Position = new Vector3(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z + 0.2f);

                    vehicle.PlaceOnGround();

                    vehicle.IsCollisionEnabled = true;

                    await BaseScript.Delay(1000);

                    await vehicle.FadeIn();
                }
            }
        }

        [CommandInfo(new[] { "owner", "reg", "registration" })]
        public class PlayerVehicleOwner : ICommand
        {
            public void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = Cache.PlayerPed.GetVehicleInFront();

                string nameOfOwner = vehicle.State.Get(StateBagKey.PLAYER_NAME) ?? string.Empty;
                bool isMissionVehicle = vehicle.State.Get(StateBagKey.VEHICLE_MISSION) ?? false;

                if (string.IsNullOrEmpty(nameOfOwner))
                {
                    Notify.Impound($"Vehicle Owner", "Sorry, we cannot find that information right now.");
                    return;
                }

                Notify.Impound($"Registration", $"~b~Owner~s~: {nameOfOwner}~n~~b~Mission Item~s~: {isMissionVehicle}");
            }
        }
    }
}
