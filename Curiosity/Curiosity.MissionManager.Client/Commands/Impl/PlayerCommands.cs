﻿using CitizenFX.Core;
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

        [CommandInfo(new[] { "coroner", "c" })]
        public class PlayerCoroner : ICommand
        {
            public void OnAsync(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments)
            {
                List<Ped> peds = World.GetAllPeds().Where(x => x.IsDead && x.IsInRangeOf(Game.PlayerPed.Position, 15f)).ToList();

                peds.ForEach(async p =>
                {
                    p.MarkAsNoLongerNeeded();
                    await p.FadeOut();
                    p.Delete();

                    int pedHandle = p.Handle;

                    if (p.Exists())
                    {
                        API.RemovePedElegantly(ref pedHandle);
                        API.DeleteEntity(ref pedHandle);
                    }
                });
            }
        }

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

                bool vehicleDeletionSent = await EventSystem.Request<bool>("vehicle:tow", vehicle.NetworkId);

                if (vehicleDeletionSent)
                {
                    Notify.Impound($"Vehicle Impounded", "~b~Charge: ~g~$1000~n~~w~Pleasure doing business with ye");
                }
                else
                {
                    Notify.Impound($"We have an issue", "We cannot tow this vehicle, someone owns this.");
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
