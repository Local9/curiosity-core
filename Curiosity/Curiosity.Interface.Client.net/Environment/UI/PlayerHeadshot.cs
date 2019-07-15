using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Interface.Client.net.Environment.UI
{
    class PlayerHeadshot
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterTickHandler(RenderHeadshot);
        }

        static async Task RenderHeadshot()
        {
            int registeredHeadshot = RegisterPedheadshot(Client.PedHandle);
            
            while (!IsPedheadshotReady(registeredHeadshot))
                await BaseScript.Delay(100);

            string headshotTxd = GetPedheadshotTxdString(registeredHeadshot);

            while (!Client.hideHud)
            {
                await BaseScript.Delay(10);
                DrawSprite(headshotTxd, headshotTxd, 0.9f, 0.15f, 0.03f, 0.06f, 0.0f, 255, 255, 255, 255);
            }
        }
    }
}
