using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment;
using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Client.Package;
using Atlas.Roleplay.Library;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Atlas.Roleplay.Client.Commands.Impl
{
    public class RoleplayChat
    {
        public class Twitter : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedTwitter : ICommand
            {
                public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
                {
                    Chat.SendGlobalMessage(player.Character.Fullname, string.Join(" ", arguments),
                        Color.FromArgb(50, 50, 255));
                }
            }

            public override string[] Aliases { get; set; } = {"twitter", "twt", "tweet"};
            public override string Title { get; set; } = "Twitter";
            public override Color Color { get; set; } = Color.FromArgb(50, 50, 255);
            public override bool IsRestricted { get; set; }
        }

        public class Me : CommandContext
        {
            [CommandInfo(new string[0])]
            public class NestedMe : ICommand
            {
                public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
                {
                    var manager = ActionMessageManager.GetModule();

                    manager.Messages.Add(new ActionMessageManager.ActionMessage
                    {
                        Seed = Seed.Generate(),
                        Sender = API.GetPlayerServerId(API.PlayerId()),
                        Message = string.Join(" ", arguments)
                    });
                    manager.Commit();
                }
            }

            public override string[] Aliases { get; set; } = {"me", "do"};
            public override string Title { get; set; } = "";
            public override Color Color { get; set; } = Color.FromArgb(255, 0, 0);
            public override bool IsRestricted { get; set; }
        }

        public class ActionMessageManager : Manager<ActionMessageManager>
        {
            public List<ActionMessage> Messages { get; set; } = new List<ActionMessage>();

            public override void Begin()
            {
                NetworkPackage.GetModule().Imports += (package, index) =>
                {
                    if (index != "World.Items.Dropped") return;

                    Messages = package.GetLoad<List<ActionMessage>>("Ingame.Action.Messages").Get() ??
                               new List<ActionMessage>();
                };
            }

            public void Commit()
            {
                NetworkPackage.GetModule()
                    .GetLoad<List<ActionMessage>>("Ingame.Action.Messages")
                    .UpdateAndCommit(Messages);
            }

            [TickHandler(SessionWait = true)]
            private async Task OnTick()
            {
                var removal = new List<ActionMessage>();

                foreach (var message in Messages)
                {
                    message.Ticks++;

                    if (message.Ticks >= 15000)
                    {
                        removal.Add(message);

                        continue;
                    }

                    if (string.IsNullOrEmpty(message.Message)) continue;

                    var ped = API.GetPlayerPed(API.GetPlayerFromServerId(message.Sender));

                    if (!API.DoesEntityExist(ped)) continue;

                    WorldText.Draw($"* {message.Message}", 1f, API.GetEntityCoords(ped, false).ToPosition());
                }

                foreach (var entry in removal)
                {
                    Messages.RemoveAll(self => self.Seed == entry.Seed);
                }

                await Task.FromResult(0);
            }

            public class ActionMessage
            {
                public string Seed { get; set; }
                public int Sender { get; set; }
                public string Message { get; set; }
                [JsonIgnore] public long Ticks { get; set; }
            }
        }
    }
}