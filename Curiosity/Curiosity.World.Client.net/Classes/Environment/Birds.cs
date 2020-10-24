using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Classes.Environment
{
    class Birds
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterTickHandler(OnTick);
        }

        static async Task OnTick()
        {
            CitizenFX.Core.Native.API.SetGlobalMinBirdFlightHeight(20.0f);
        }
    }
}
