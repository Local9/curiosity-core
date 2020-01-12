using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Curiosity.System.Library;
using Curiosity.System.Library.Events;
using Curiosity.System.Library.Inventory;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using Curiosity.System.Server.Extensions;
using Curiosity.System.Server.MySQL;

namespace Curiosity.System.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("characters:find", new EventCallback(metadata =>
            {
                using (var context = new StorageContext())
                {
                    var user = Curiosity.Lookup(metadata.Sender);
                    var result = context.Characters.Where(self => self.Owner == user.Seed)
                        .ToList();

                    user.Characters.Clear();
                    user.Characters.AddRange(result);

                    Logger.Debug(
                        $"[Characters] [{user.Seed}] Fetched all characters for `{user.LastName}` totalling {result.Count} character(s).");

                    return result;
                }
            }));

            EventSystem.Attach("characters:delete", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);
                var user = Curiosity.Lookup(metadata.Sender);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    var character = context.Characters.First(self => self.Seed == seed);

                    user.Characters.Add(character);
                    context.Characters.Remove(character);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }


                return null;
            }));

            EventSystem.Attach("characters:create", new AsyncEventCallback(async metadata =>
            {
                var user = Curiosity.Lookup(metadata.Sender);
                var random = new Random();
                var character = new CuriosityCharacter
                {
                    Seed = Seed.Generate(),
                    Owner = user.Seed,
                    Name = metadata.Find<string>(0),
                    Surname = metadata.Find<string>(1),
                    DateOfBirth = metadata.Find<string>(2),
                    LastDigits = random.Next(1000, 10000),
                    Health = 100,
                    Shield = 0,
                    Cash = 3000,
                    Style = new Style(),
                    BankAccount = new BankAccount
                    {
                        Balance = 0,
                        History = new List<BankTransaction>()
                    },
                    Metadata = new CharacterMetadata
                    {
                        SavedOutfits = new Dictionary<string, Style>(),
                        Inventories = new List<InventoryContainerBase>(),
                    }
                };

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    while (context.Characters.FirstOrDefault(self =>
                               self.DateOfBirth == character.DateOfBirth && self.LastDigits == character.LastDigits) !=
                           null)
                    {
                        character.LastDigits = random.Next(1000, 10000);
                    }

                    context.Characters.AddOrUpdate(character);

                    await context.SaveChangesAsync();

                    transaction.Commit();

                    Logger.Info(
                        $"[Characters] [{user.LastName}] Has created a new character named `{character.Name} {character.Surname}` ({character.DateOfBirth.Replace("-", "") + character.LastDigits}) ({character.Name})");
                }

                user.Characters.Add(character);

                return character;
            }));

            EventSystem.Attach("characters:save", new AsyncEventCallback(async metadata =>
            {
                var character = metadata.Find<CuriosityCharacter>(0);

                await character.Save();

                return null;
            }));

            EventSystem.Attach("characters:fetch", new EventCallback(metadata =>
            {
                var fullname = metadata.Find<string>(0).ToLower().Trim();

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if (character.Fullname.Trim().ToLower() == fullname) return character;
                    }
                }

                return null;
            }));
            
            EventSystem.Attach("characters:fetchbyseed", new EventCallback(metadata =>
            {
                var seed = metadata.Find<string>(0);

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if (character.Seed == seed) return character;
                    }
                }

                return null;
            }));

            EventSystem.Attach("characters:fetchbyssn", new EventCallback(metadata =>
            {
                var ssn = metadata.Find<string>(0).Trim();

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if (new string((character.DateOfBirth + character.LastDigits).Replace("-", "").Skip(2).ToArray()) == ssn)
                            return character;
                    }
                }

                return null;
            }));
        }
    }
}