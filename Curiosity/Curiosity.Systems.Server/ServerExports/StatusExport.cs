using CitizenFX.Core;
using Curiosity.Systems.Server.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.ServerExports
{
    public class StatusExport : Manager<StatusExport>
    {
        public override void Begin()
        {
            Curiosity.ExportDictionary.Add("Status", new Func<string>(
                () =>
                {
                    var returnObject = new { status = CuriosityPlugin.ServerReady };
                    return JsonConvert.SerializeObject(returnObject);
                }
            ));
        }
    }
}
