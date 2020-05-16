using CitizenFX.Core.Native;
using Curiosity.System.Client.Diagnostics;
using Curiosity.System.Client.Interface;
using Curiosity.System.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Curiosity.System.Client.Commands
{
    public class CommandFramework
    {
        public Dictionary<CommandContext, List<Tuple<CommandInfo, ICommand>>> Registry { get; set; } =
            new Dictionary<CommandContext, List<Tuple<CommandInfo, ICommand>>>();

        public void Bind(Type type)
        {
            if (type.BaseType == null || type.BaseType != typeof(CommandContext))
            {
                Logger.Info(
                    $"[CommandFramework] The binding of `{type.Name}` could not be completed due to the lack of the `CommandContext` implementation.");

                return;
            }

            var context = (CommandContext)Activator.CreateInstance(type);
            var target = typeof(ICommand);
            var assembly = type.Assembly;
            var found = assembly.GetExportedTypes()
                .Where(self =>
                    self.DeclaringType != null && self != target && target.IsAssignableFrom(self) && self.IsNested &&
                    self.DeclaringType.FullName == type.FullName)
                .ToList();
            var registered = 0;

            Registry.Add(context, new List<Tuple<CommandInfo, ICommand>>());

            foreach (var nested in found)
            {
                if (!(nested.GetCustomAttributes(typeof(CommandInfo), true).FirstOrDefault() is CommandInfo commandInfo)
                ) continue;

                var created = (ICommand)Activator.CreateInstance(nested);

                CuriosityPlugin.Instance.AttachTickHandlers(created);

                Registry[context].Add(new Tuple<CommandInfo, ICommand>(commandInfo, created));

                registered++;
            }

            context.Aliases.ToList().ForEach(self =>
                API.RegisterCommand(self,
                    new Action<int, List<object>, string>((handle, args, raw) =>
                        HandleCommandInput(context, Registry[context], self, args)), false));

            Logger.Info(
                $"[CommandFramework] Found {found.Count} nested `ICommand` class(es) in `{type.Name}`, registered {registered} of them!");
        }

        private void HandleCommandInput(CommandContext context,
            IReadOnlyCollection<Tuple<CommandInfo, ICommand>> registry,
            string alias,
            IReadOnlyList<object> arguments)
        {
            var player = Cache.Player;

            if (context.IsRestricted && player.User.Role != Role.Admin)
            {
                Chat.SendLocalMessage("Säkerhet", "Du har inte Administrator rättigheter!", Color.FromArgb(255, 0, 0));

                return;
            }

            foreach (var entry in registry)
            {
                if (entry.Item1.Aliases.Length >= 1) continue;

                entry.Item2.On(player, player.Entity, arguments.Skip(entry.Item1.Aliases.Length > 1 ? 1 : 0).Select(self => self.ToString()).ToList());

                return;
            }

            if (arguments.Count < 1)
            {
                Chat.SendLocalMessage(context.Title, "Alla kommandon:", context.Color);

                foreach (var entry in registry)
                {
                    Chat.SendLocalMessage($"/{alias} {string.Join(", ", entry.Item1.Aliases)}",
                        Color.FromArgb(255, 255, 255));
                }

                return;
            }

            var subcommand = arguments[0];
            var matched = false;

            foreach (var entry in registry)
            {
                if (!entry.Item1.Aliases.Select(self => self.ToLower())
                    .Contains(subcommand.ToString().ToLower())) continue;

                entry.Item2.On(player, player.Entity, arguments.Skip(1).Select(self => self.ToString()).ToList());

                matched = true;

                break;
            }

            if (!matched)
                Chat.SendLocalMessage(context.Title, $"Kunde inte hitta kommando: {subcommand}", context.Color);
        }
    }
}