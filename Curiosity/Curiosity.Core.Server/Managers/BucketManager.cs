using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class BucketManager : Manager<BucketManager>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("BucketLockdownMode", new Func<int, string, string>((bucketId, mode) =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (bucketId == 0 || bucketId >= 5000)
                {
                    exportMessage.error = "Lobby and spawn range is is protected";
                }
                else
                {
                    if (mode == "strict" || mode == "relaxed" || mode == "inactive")
                    {
                        API.SetRoutingBucketEntityLockdownMode(bucketId, mode);
                    }
                    else
                    {
                        exportMessage.error = $"Unknown mode '{mode}', can only accept strict, relaxed, or inactive";
                    }
                }

                return $"{exportMessage}";
            }));

            Instance.ExportDictionary.Add("BucketPopulationEnabled", new Func<int, bool, string>((bucketId, enabled) =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (bucketId == 0 || bucketId >= 5000)
                {
                    exportMessage.error = "Lobby and spawn range is is protected";
                }
                else
                {
                    API.SetRoutingBucketPopulationEnabled(bucketId, enabled);
                }

                return $"{exportMessage}";
            }));
        }
    }
}
