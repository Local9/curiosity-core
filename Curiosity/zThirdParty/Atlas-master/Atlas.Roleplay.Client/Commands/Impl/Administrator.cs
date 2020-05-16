using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Inventory;
using Atlas.Roleplay.Client.Inventory.Items;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Atlas.Roleplay.Client.Commands.Impl
{
    public class Administrator : CommandContext
    {
        #region Jobb

        [CommandInfo(new[] { "job" })]
        public class Job : ICommand
        {
            public const string Title = "Administratör";
            public const string Usage = "Användning: /admin job set <jobb> <roll>";

            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 1)
                {
                    Chat.SendLocalMessage(Title, Usage, Color.FromArgb(255, 0, 0));

                    return;
                }

                switch (arguments.ElementAt(0).ToUpper())
                {
                    case "SET":
                        if (arguments.Count < 3)
                        {
                            Chat.SendLocalMessage(Title, Usage, Color.FromArgb(255,
                                0, 0));

                            return;
                        }

                        var jobName = arguments[1];
                        var jobRole = 0;

                        try
                        {
                            jobRole = int.Parse(arguments[2]);
                        }
                        catch (Exception)
                        {
                            // Ignored
                        }

                        Enum.TryParse<Employment>(jobName, true, out var job);

                        var character = Cache.Character;

                        character.Metadata.Employment = job;
                        character.Metadata.EmploymentRole = jobRole;

                        Chat.SendLocalMessage(Title,
                            $"Anställde {character.Fullname} hos {job.ToString()} - {jobRole}",
                            Color.FromArgb(255, 0, 0));

                        break;
                    default:
                        Chat.SendLocalMessage(Title,
                            $"Kunde inte hitta kommando: {arguments.ElementAt(0)}", Color.FromArgb(255, 0, 0));

                        break;
                }
            }
        }

        #endregion

        #region Pengar

        [CommandInfo(new[] { "cash" })]
        public class Cash : ICommand
        {
            public const string Title = "Administratör";
            public const string Usage = "Användning: /admin cash <add, set, remove> <antal pengar>";
            public const string InvalidAmount = "Antalet pengar måste vara ett nummer.";
            public const string NotFoundSubcommand = "Kunde inte hitta cash kommando: ";

            public const string CorrectedCash =
                "Dina fickpengar har rättats till utav en administratör. (SEK ";

            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 2)
                {
                    Chat.SendLocalMessage(Title, Usage, Color.FromArgb(255, 0, 0));

                    return;
                }

                int balance;

                try
                {
                    balance = Convert.ToInt32(arguments.ElementAt(1));
                }
                catch (FormatException)
                {
                    Chat.SendLocalMessage(Title, InvalidAmount, Color.FromArgb(255, 0, 0));

                    return;
                }

                switch (arguments.ElementAt(0).ToUpper())
                {
                    case "ADD":
                        player.Character.Cash += balance;

                        Chat.SendLocalMessage(Title,
                            $"{CorrectedCash}{player.Character.Cash})", Color.FromArgb(255, 0, 0));

                        break;
                    case "SET":
                        player.Character.Cash = balance;
                        Chat.SendLocalMessage(Title,
                            $"{CorrectedCash}{player.Character.Cash})", Color.FromArgb(255, 0, 0));

                        break;
                    case "REMOVE":
                        player.Character.Cash -= balance;
                        Chat.SendLocalMessage(Title,
                            $"{CorrectedCash}{player.Character.Cash})", Color.FromArgb(255, 0, 0));

                        break;
                    default:
                        Chat.SendLocalMessage(Title,
                            $"{NotFoundSubcommand}{arguments.ElementAt(0)}", Color.FromArgb(255, 0, 0));

                        break;
                }
            }
        }

        [CommandInfo(new[] { "bank" })]
        public class Bank : ICommand
        {
            public const string Title = "Administratör";
            public const string Usage = "Användning: /admin bank <add, set, remove> <antal pengar>";
            public const string InvalidAmount = "Antalet pengar måste vara ett nummer.";
            public const string NotFoundSubcommand = "Kunde inte hitta cash kommando: ";

            public const string CorrectedBankAccount =
                "Ditt bankkonto har rättats till utav en administratör. (SEK ";

            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 2)
                {
                    Chat.SendLocalMessage(Title, Usage, Color.FromArgb(255, 0, 0));

                    return;
                }

                int balance;

                try
                {
                    balance = Convert.ToInt32(arguments.ElementAt(1));
                }
                catch (FormatException)
                {
                    Chat.SendLocalMessage(Title, InvalidAmount, Color.FromArgb(255, 0, 0));

                    return;
                }

                switch (arguments.ElementAt(0).ToUpper())
                {
                    case "ADD":
                        player.Character.BankAccount.Balance += balance;
                        Chat.SendLocalMessage(Title,
                            $"{CorrectedBankAccount}{player.Character.BankAccount.Balance}", Color.FromArgb(255, 0, 0));

                        break;
                    case "SET":
                        player.Character.BankAccount.Balance = balance;
                        Chat.SendLocalMessage(Title,
                            $"{CorrectedBankAccount}{player.Character.BankAccount.Balance}", Color.FromArgb(255, 0, 0));

                        break;
                    case "REMOVE":
                        player.Character.BankAccount.Balance -= balance;
                        Chat.SendLocalMessage(Title,
                            $"{CorrectedBankAccount}{player.Character.BankAccount.Balance}", Color.FromArgb(255, 0, 0));

                        break;
                    default:
                        Chat.SendLocalMessage(Title,
                            $"{NotFoundSubcommand}{arguments.ElementAt(0)}", Color.FromArgb(255, 0, 0));

                        break;
                }
            }
        }

        #endregion

        #region Spelare

        [CommandInfo(new[] { "revive", "rev" })]
        public class Revive : ICommand
        {
            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                player.Character.Revive(entity.Position);
            }
        }

        #endregion

        #region Inventory

        [CommandInfo(new[] { "item", "giveitem", "give" })]
        public class ItemCommand : ICommand
        {
            // More complex solution later, like getting all registered items.
            public Dictionary<string, Type> Items { get; set; } = new Dictionary<string, Type>
            {
                ["bandage"] = typeof(BandageItem)
            };

            public void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments)
            {
                if (arguments.Count < 1)
                {
                    return;
                }

                var argument = arguments[0];

                foreach (var item in Items)
                {
                    if (item.Key != argument.ToLower()) continue;

                    ItemHelper.Give(InventoryManager.GetModule().GetContainer("pockets_inventory"),
                        (InventoryItem)Activator.CreateInstance(item.Value));

                    Chat.SendLocalMessage("Föremål", $"Gav dig x1 utav `{item.Key}`...", Color.FromArgb(0, 255, 0));
                    break;
                }
            }
        }

        #endregion

        public override string[] Aliases { get; set; } = { "admin" };
        public override string Title { get; set; } = "Administratör";
        public override Color Color { get; set; } = Color.FromArgb(255, 0, 0);
        public override bool IsRestricted { get; set; } = true;
    }
}