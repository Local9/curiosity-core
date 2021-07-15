using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Core.Server.Web.Discord.Entity;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

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
                    exportMessage.Error = "Lobby and spawn range is is protected";
                }
                else
                {
                    if (mode == "strict" || mode == "relaxed" || mode == "inactive")
                    {
                        API.SetRoutingBucketEntityLockdownMode(bucketId, mode);
                    }
                    else
                    {
                        exportMessage.Error = $"Unknown mode '{mode}', can only accept strict, relaxed, or inactive";
                    }
                }

                return $"{exportMessage}";
            }));

            Instance.ExportDictionary.Add("BucketLockdownMode", new Func<int, bool, string>((bucketId, enabled) =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (bucketId == 0 || bucketId >= 5000)
                {
                    exportMessage.Error = "Lobby and spawn range is is protected";
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
