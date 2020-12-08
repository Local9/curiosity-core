using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Commands.Impl
{
    public class PlayerCommands : CommandContext
    {
        static EventSystem EventSystem => EventSystem.GetModule();
        public override string[] Aliases { get; set; } = { "dispatch", "pd" };
        public override string Title { get; set; } = "Player Commands";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = false;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { };

        [CommandInfo(new[] { "tow" })]
        public class PlayerTow : ICommand
        {
            public async void OnAsync(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = World.GetAllVehicles().Select(x => x).Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 4f) && (x.Driver == null || x.Driver.IsDead)).FirstOrDefault();

                if (vehicle == null)
                {
                    Notify.Impound($"Invalid Information", $"Sorry bud, cannot find ere on this ere computer that there vehicle.");
                    return;
                }

                int vehicleDeletionSent = await EventSystem.Request<int>("vehicle:tow", vehicle.NetworkId);
                CommonErrors commonErrors = (CommonErrors)vehicleDeletionSent;

                switch(commonErrors)
                {
                    case CommonErrors.PurchaseSuccessful:
                        Notify.Impound($"Vehicle Impounded", "~b~Charge: ~g~$1000~n~~w~Pleasure doing business with ye");
                        break;
                    case CommonErrors.PurchaseUnSuccessful:
                        Notify.Impound($"Payment Issue", "Looks like ur bank rejected it.");
                        break;
                    case CommonErrors.VehicleIsOwned:
                        Notify.Impound($"Computer Warning", "Sorry bub, this vehicle is owned by someone.");
                        break;
                    case CommonErrors.NotEnoughPoliceRep1000:
                        Notify.Alert(commonErrors);
                        break;
                    default:
                        Notify.Impound($"Computer Error", "Computer said no...");
                        break;
                }
            }
        }

        [CommandInfo(new[] { "owner", "reg", "registration" })]
        public class PlayerVehicleOwner : ICommand
        {
            public async void OnAsync(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                Vehicle vehicle = World.GetAllVehicles().Select(x => x).Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 4f)).FirstOrDefault();

                var nameOfOwner = await EventSystem.Request<object>("vehicle:owner", vehicle.NetworkId);

                string nameValue = nameOfOwner.ToString();

                if (string.IsNullOrEmpty(nameValue))
                {
                    Notify.Impound($"Vehicle Owner", "Sorry, we cannot find that information right now.");
                }
                else
                {
                    Notify.Impound($"Registration", $"~b~Owner~s~: {nameValue}");
                }
            }
        }

    }
}
