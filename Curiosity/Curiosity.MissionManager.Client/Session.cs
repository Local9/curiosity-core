using CitizenFX.Core;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public class Session
    {
        public static async Task Loading()
        {
            while (true)
            {
                if (Cache.Player?.Character != null && Cache.Entity != null) break;

                await BaseScript.Delay(100);
            }
        }
    }
}
