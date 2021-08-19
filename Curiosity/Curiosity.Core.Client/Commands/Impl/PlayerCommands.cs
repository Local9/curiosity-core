using CitizenFX.Core;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Drawing;

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

        [CommandInfo(new[] { "unstuck", })]
        public class Unstuck : ICommand
        {
            public async void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Position position = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);
                await SafeTeleport.TeleportFadePlayer(position);
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
