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
            while (true)
            {
                CitizenFX.Core.Native.API.SetGlobalMinBirdFlightHeight(20.0f);
                await Client.Delay(0);
            }
        }
    }
}
