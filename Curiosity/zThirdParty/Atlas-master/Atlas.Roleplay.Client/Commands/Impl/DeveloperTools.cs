using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Atlas.Roleplay.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        #region Fordon

        [CommandInfo(new[] { "vehicle", "veh", "car" })]
        public class VehicleSpawner : ICommand
        {
            public async void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                try
                {
                    if (arguments.Count <= 0) return;

                    var model = new Model(API.GetHashKey(arguments.ElementAt(0)));

                    if (!model.IsValid || !model.IsVehicle) return;

                    var position = entity.Position;
                    var vehicle = await World.CreateVehicle(model, position.AsVector(), position.Heading);

                    entity.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                }
                catch (Exception)
                {
                    // Ignored
                }
            }
        }

        [CommandInfo(new[] { "repair", "fix", "wash" })]
        public class VehicleRepairer : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                var vehicle = entity.Vehicle;

                if (vehicle == null) return;

                vehicle.Wash();
                vehicle.Repair();
                vehicle.PlaceOnGround();
            }
        }

        [CommandInfo(new[] { "dv", "deleteveh" })]
        public class VehicleDespawner : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                entity.Vehicle?.Delete();
            }
        }

        #endregion

        #region Teleporterings verktyg

        [CommandInfo(new[] { "tpm" })]
        public class TeleportMarker : ICommand
        {
            public async void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                var waypoint = World.GetWaypointBlip();

                if (waypoint == null) return;

                var position = waypoint.Position;

                position.Z = World.GetGroundHeight(position) + 1;

                await player.Entity.Teleport(position.ToPosition());
            }
        }

        #endregion

        #region Utvecklings verktyg

        [CommandInfo(new[] { "position", "pos", "coords" })]
        public class PositionLogger : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                var position = entity.Position;
                var title = arguments.Count > 1 ? string.Join(" ", arguments.Skip(1)) : "Position";
                var log =
                    $"[Developer] {title}: new Position({position.X}f, {position.Y}f, {position.Z}f, {position.Heading}f)";

                Logger.Info(log);
            }
        }

        [CommandInfo(new[] { "style" })]
        public class StyleLogger : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                StyleManager.GetModule().OpenStyleChange(Cache.Character.Style, "General", 0,
                    (type) =>
                    {
                        if (type != 0) return;

                        Logger.Info($"[Developer] {JsonConvert.SerializeObject(player.Character.Style)}");
                    }, "All");
            }
        }

        [CommandInfo(new[] { "session" })]
        public class SessionSwitcher : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 1)
                {
                    Chat.SendLocalMessage("Session", $"{Session.LastSession}", Color.FromArgb(0, 0, 255));

                    return;
                }

                int id;

                try
                {
                    id = int.Parse(arguments.ElementAt(0));
                }
                catch (Exception)
                {
                    id = 0;
                }

                Session.Join(id);

                Chat.SendLocalMessage("Session", $"Anslöt till session #{id}", Color.FromArgb(0, 0, 255));
            }
        }

        [CommandInfo(new[] { "voice" })]
        public class VoiceChatSwitcher : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 1)
                {
                    Chat.SendLocalMessage("Voice Chat",
                        Function.Call<float>(Hash.NETWORK_GET_TALKER_PROXIMITY).ToString(CultureInfo.InvariantCulture),
                        Color.FromArgb(0, 0, 255));

                    return;
                }

                float range;

                try
                {
                    range = float.Parse(arguments.ElementAt(0));
                }
                catch (Exception)
                {
                    range = 0f;
                }

                Function.Call(Hash.NETWORK_CLEAR_VOICE_CHANNEL);
                Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, range);

                Chat.SendLocalMessage("Voice Chat", $"Ändrade voicechat proximity till {range}",
                    Color.FromArgb(0, 0, 255));
            }
        }

        [CommandInfo(new[] { "anim", "animastion" })]
        public class Anim : ICommand
        {
            public async void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 2)
                {
                    return;
                }

                await entity.Task.PlayAnimation(arguments[0], arguments[1], 8f, -8f, -1, AnimationFlags.None, 0);
            }
        }

        #endregion

        public override string[] Aliases { get; set; } = { "dev", "developer" };
        public override string Title { get; set; } = "Utvecking";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = true;
    }
}