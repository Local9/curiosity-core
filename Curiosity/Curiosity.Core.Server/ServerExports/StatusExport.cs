using Curiosity.Core.Server.Managers;
using Newtonsoft.Json;
using System;

namespace Curiosity.Core.Server.ServerExports
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
