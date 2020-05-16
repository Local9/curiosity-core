using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Server.Extensions;
using System.Collections.Generic;

namespace Atlas.Roleplay.Server.Managers
{
    public class NetworkPackageManager : Manager<NetworkPackageManager>
    {
        public Dictionary<string, string> LatestMigration { get; set; } = new Dictionary<string, string>();

        public override void Begin()
        {
            EventSystem.Attach("network:package:update", new EventCallback(metadata =>
            {
                var index = metadata.Find<string>(0);
                var payload = metadata.Find<string>(1);

                foreach (var user in Atlas.ActiveUsers)
                {
                    if (user.Handle == metadata.Sender) continue;

                    user.Send("network:package:receive", index, payload);
                }

                LatestMigration[index] = payload;

                return null;
            }));

            EventSystem.Attach("network:package:latest", new EventCallback(metadata => LatestMigration));
        }
    }
}