using CitizenFX.Core.Native;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Environment.Entities;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Police.Client.Commands
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

                PluginManager.Instance.AttachTickHandlers(created);

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
            CuriosityPlayer player = Cache.CuriosityPlayer;

            Role playerRole = (Role)player.Player.State.Get(StateBagKey.PLAYER_ROLE);

            if (context.IsRestricted && !context.RequiredRoles.Contains(playerRole))
            {
                return;
            }

            foreach (var entry in registry)
            {
                if (entry.Item1.Aliases.Length >= 1) continue;

                entry.Item2.On(player, arguments.Skip(entry.Item1.Aliases.Length > 1 ? 1 : 0).Select(self => self.ToString()).ToList());

                return;
            }

            //if (arguments.Count < 1)
            //{
            //    Chat.SendLocalMessage("Avaliable Commands:", "help");

            //    foreach (var entry in registry)
            //    {
            //        Chat.SendLocalMessage($"/{alias} {string.Join(", ", entry.Item1.Aliases)}", "help");
            //    }

            //    return;
            //}

            var subcommand = arguments[0];
            var matched = false;

            foreach (var entry in registry)
            {
                if (!entry.Item1.Aliases.Select(self => self.ToLower())
                    .Contains(subcommand.ToString().ToLower())) continue;

                entry.Item2.On(player, arguments.Skip(1).Select(self => self.ToString()).ToList());

                matched = true;

                break;
            }

            //if (!matched) // NEED TO FIRE ON THE SERVER!
            //    Chat.SendLocalMessage($"Command not found: {subcommand}", "help");
        }
    }
}