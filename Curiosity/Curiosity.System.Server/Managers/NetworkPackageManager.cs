using System.Collections.Generic;
using Curiosity.System.Library.Events;
using Curiosity.System.Server.Extensions;

namespace Curiosity.System.Server.Managers
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

                foreach (var user in Curiosity.ActiveUsers)
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