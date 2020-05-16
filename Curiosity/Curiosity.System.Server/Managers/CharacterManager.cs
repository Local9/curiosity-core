using Curiosity.System.Library.Events;
using Curiosity.System.Library.Inventory;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using Curiosity.System.Server.Extensions;
using Curiosity.System.Server.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    var result = context.Characters.Where(self => self.UserId == user.UserId)
                        .ToList();

                    user.Characters.Clear();
                    user.Characters.AddRange(result);

                    Logger.Debug(
                        $"[Characters] [{user.UserId}] Fetched all characters for `{user.LastName}` totalling {result.Count} character(s).");

                    return result;
                }
            }));

            EventSystem.Attach("characters:delete", new AsyncEventCallback(async metadata =>
            {
                var characterId = metadata.Find<int>(0);
                var user = Curiosity.Lookup(metadata.Sender);

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    var character = context.Characters.First(self => self.CharacterId == characterId);

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
                    context.Characters.AddOrUpdate(character);

                    await context.SaveChangesAsync();

                    transaction.Commit();

                    Logger.Info(
                        $"[Characters] [{user.LastName}] Has created a new character.");
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

            //EventSystem.Attach("characters:fetch", new EventCallback(metadata =>
            //{
            //    var fullname = metadata.Find<string>(0).ToLower().Trim();

            //    using (var context = new StorageContext())
            //    {
            //        foreach (var character in context.Characters)
            //        {
            //            if (character.Fullname.Trim().ToLower() == fullname) return character;
            //        }
            //    }

            //    return null;
            //}));

            EventSystem.Attach("characters:fetchByCharacterId", new EventCallback(metadata =>
            {
                var characterId = metadata.Find<int>(0);

                using (var context = new StorageContext())
                {
                    foreach (var character in context.Characters)
                    {
                        if (character.CharacterId == characterId) return character;
                    }
                }

                return null;
            }));
        }
    }
}