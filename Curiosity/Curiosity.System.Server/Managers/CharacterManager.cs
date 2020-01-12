using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Atlas.Roleplay.Library;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.LawEnforcement;
using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Server.Diagnostics;
using Atlas.Roleplay.Server.Extensions;
using Atlas.Roleplay.Server.MySQL;

namespace Atlas.Roleplay.Server.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("characters:find", new EventCallback(metadata =>
            {
                using (var context = new StorageContext())
                {
                    var user = Atlas.Lookup(metadata.Sender);
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
                var user = Atlas.Lookup(metadata.Sender);

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
                var user = Atlas.Lookup(metadata.Sender);
                var random = new Random();
                var character = new AtlasCharacter
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
                        Employment = Employment.Unemployed,
                        SavedOutfits = new Dictionary<string, Style>(),
                        Inventories = new List<InventoryContainerBase>(),
                        JailCases = new List<JailCase>()
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
                var character = metadata.Find<AtlasCharacter>(0);

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