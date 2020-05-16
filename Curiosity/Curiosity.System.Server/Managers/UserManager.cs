using CitizenFX.Core;
using Curiosity.System.Library.Events;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using Curiosity.System.Server.Extensions;
using Curiosity.System.Server.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.System.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = CuriosityPlugin.PlayersList[metadata.Sender];
                var discordIdStr = player.Identifiers["discord"];
                var license = player.Identifiers["license"];
                ulong discordId = 0;

                if (!ulong.TryParse(discordIdStr, out discordId))
                {
                    player.Drop("Error creating login session, Discord ID not found.");
                    return default;
                }

                var connectedBefore = true;

                using (var context = new StorageContext())
                using (var transaction = context.BeginTransaction())
                {
                    var user = context.Users.FirstOrDefault(self => self.DiscordId == discordId);
                    var connection = new Tuple<string, DateTime>(player.EndPoint, DateTime.Now);

                    if (user == null)
                    {
                        user = new CuriosityUser
                        {
                            License = license,
                            DiscordId = discordId,
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
                        ? $"[User] [{user.UserId}] [{user.LastName}] Has connected to the server ({connection.Item1})"
                        : $"[User] [{user.UserId}] [{player.Name}] Has connected for the first time ({connection.Item1})");

                    Curiosity.ActiveUsers.Add(user);

                    return user;
                }
            }));

            EventSystem.Attach("user:save", new AsyncEventCallback(async metadata =>
            {
                await Curiosity.Lookup(metadata.Sender).Save();

                return null;
            }));

            EventSystem.Attach("user:fetch", new EventCallback(metadata =>
            {
                var userId = metadata.Find<long>(0);

                return Curiosity.ActiveUsers.FirstOrDefault(self => self.UserId == userId);
            }));

            EventSystem.Attach("user:postupdates", new EventCallback(metadata =>
            {
                var user = metadata.Find<CuriosityUser>(0);

                Curiosity.ActiveUsers.RemoveAll(self => self.UserId == user.UserId);
                Curiosity.ActiveUsers.Add(user);

                return null;
            }));

            EventSystem.Attach("user:redirect", new EventCallback(metadata =>
            {
                EventSystem.Send(metadata.Find<string>(1), metadata.Find<int>(0), metadata.AsEnumerable().Skip(2).ToArray());

                return null;
            }));

            Curiosity.EventRegistry["playerDropped"] += new Action<Player, string>(OnUserDisconnect);
        }

        private void OnUserDisconnect([FromSource] Player player, string reason)
        {
            var user = Curiosity.Lookup(Convert.ToInt32(player.Handle));

            if (user == null) return;

            Logger.Info($"[User] [{user.UserId}] [{user.LastName}] Has disconnected from the server, saving ({reason}).");

            Task.Factory.StartNew(async () => await Curiosity.SaveOperation(user));
        }
    }
}