using Curiosity.Systems.Server.Managers;
using Newtonsoft.Json;
using System;

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
