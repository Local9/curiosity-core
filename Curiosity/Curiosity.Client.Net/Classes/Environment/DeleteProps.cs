using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using System.Threading.Tasks;

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
