using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Client.net.Classes.Environment
{
    class DeleteProps
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterTickHandler(DeleteProp);
        }

        static async Task DeleteProp()
        {
            Prop p = new Prop((int)ObjectHash.prop_air_stair_01);

            if (API.DoesEntityExist(p.Handle))
            {
                p.Delete();
            }

            await Task.FromResult(0);
        }
    }
}
