using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Commands
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

            foreach (string self in context.Aliases.ToList())
            {
                Logger.Debug($"Register: {self}");

                API.RegisterCommand(self, new Action<int, List<object>, string>((handle, args, raw) =>
                        HandleCommandInput(context, Registry[context], self, args)), false);
            };

            Logger.Info(
                $"[CommandFramework] Found {found.Count} nested `ICommand` class(es) in `{type.Name}`, registered {registered} of them!");
        }

        private void HandleCommandInput(CommandContext context,
            IReadOnlyCollection<Tuple<CommandInfo, ICommand>> registry,
            string alias,
            IReadOnlyList<object> arguments)
        {
            var player = Cache.Player;

            if (context.IsRestricted && !context.RequiredRoles.Contains(player.User.Role))
            {
                Chat.SendLocalMessage("Restricted Command");
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
                Chat.SendLocalMessage("Avaliable Commands:");

                foreach (var entry in registry)
                {
                    Chat.SendLocalMessage($"/{alias} {string.Join(", ", entry.Item1.Aliases)}");
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

            if (!matched) // NEED TO FIRE ON THE SERVER!
                Chat.SendLocalMessage($"Command not found: {subcommand}");
        }
    }
}