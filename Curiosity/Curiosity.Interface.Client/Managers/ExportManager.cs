using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
{
    public class ExportManager : Manager<ExportManager>
    {
        public override void Begin()
        {
            Instance.ExportRegistry.Add("SetJobActivity", new Func<bool, bool, string, bool>(
                (active, onDuty, job) =>
                {
                    return true;
                }));
        }
    }
}
