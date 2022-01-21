using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers.Thirdparty
{
    public class ThirdparyRCoreManager : Manager<ThirdparyRCoreManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry.Add("rcore_races:giveMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:giveMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:takeMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));
        }
    }
}
