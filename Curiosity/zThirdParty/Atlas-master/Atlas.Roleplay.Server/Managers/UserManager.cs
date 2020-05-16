using Atlas.Roleplay.Library;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Server.Diagnostics;
using Atlas.Roleplay.Server.Extensions;
using Atlas.Roleplay.Server.MySQL;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = new PlayerList()[metadata.Sender];
                var steam = player.Identifiers.FirstOrDefault(self => self.StartsWith("steam:"))?.ToString();
                var connectedBefore = true;

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    var user = context.Users.FirstOrDefault(self => self.SteamId == steam);
                    var connection = new Tuple<string, DateTime>(player.EndPoint, DateTime.Now);

                    if (user == null)
                    {
                        user = new AtlasUser
                        {
                            Seed = Seed.Generate(),
                            SteamId = steam,
                            Role = Role.Member,
                            ConnectionHistory = new List<Tuple<string, DateTime>>(),
                            Metadata = new UserMetadata()
                        };

                        connectedBefore = false;
                    }

                    user.Handle = int.Parse(player.Handle);
                    user.LastName = player.Name;
                    user.LatestActivity = DateTime.Now;

                    var last = user.ConnectionHistory.LastOrDefault();

                    if (last == null || last.Item1 != connection.Item1) user.ConnectionHistory.Add(connection);

                    context.Users.AddOrUpdate(user);

                    await context.SaveChangesAsync();

                    transaction.Commit();

                    Logger.Info(connectedBefore
                        ? $"[User] [{user.Seed}] [{user.LastName}] Has connected to the server ({connection.Item1})"
                        : $"[User] [{steam}] [{player.Name}] Has connected for the first time ({connection.Item1})");

                    Atlas.ActiveUsers.Add(user);

                    return user;
                }
            }));

            EventSystem.Attach("user:save", new AsyncEventCallback(async metadata =>
            {
                await Atlas.Lookup(metadata.Sender).Save();

                return null;
            }));

            EventSystem.Attach("user:fetch", new EventCallback(metadata =>
            {
                var seed = metadata.Find<string>(0);

                return Atlas.ActiveUsers.FirstOrDefault(self => self.Seed == seed);
            }));

            EventSystem.Attach("user:postupdates", new EventCallback(metadata =>
            {
                var user = metadata.Find<AtlasUser>(0);

                Atlas.ActiveUsers.RemoveAll(self => self.Seed == user.Seed);
                Atlas.ActiveUsers.Add(user);

                return null;
            }));

            EventSystem.Attach("user:redirect", new EventCallback(metadata =>
            {
                EventSystem.Send(metadata.Find<string>(1), metadata.Find<int>(0), metadata.AsEnumerable().Skip(2).ToArray());

                return null;
            }));

            Atlas.EventRegistry["playerDropped"] += new Action<Player, string>(OnUserDisconnect);
        }

        private void OnUserDisconnect([FromSource] Player player, string reason)
        {
            var user = Atlas.Lookup(Convert.ToInt32(player.Handle));

            if (user == null) return;

            Logger.Info($"[User] [{user.Seed}] [{user.LastName}] Has disconnected from the server, saving ({reason}).");

            Task.Factory.StartNew(async () => await Atlas.SaveOperation(user));
        }
    }
}