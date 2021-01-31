using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Extensions
{
    public static class PedExtensions
    {
        public static async Task<string> GetHeadshot(this Ped ped)
        {
            int headshot = API.RegisterPedheadshot(ped.Handle);

            while (!API.IsPedheadshotReady(headshot))
            {
                await BaseScript.Delay(0);
            }

            return API.GetPedheadshotTxdString(headshot);
        }
    }
}
