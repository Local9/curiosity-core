using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.PlayerClasses
{
    class Queue
    {
        public static void Init()
        {
            Client.GetInstance().RegisterTickHandler(Connected);
        }

        static async Task Connected()
        {
            try
            {
                while (!API.NetworkIsPlayerActive(API.PlayerId()))
                {
                    await Client.Delay(5000);
                }
                Client.TriggerServerEvent("curiosity:Server:Queue:PlayerConnected");
                API.SendNuiMessage($@"{{ ""resname"" : ""{API.GetCurrentResourceName()}"" }}");
                Client.GetInstance().DeregisterTickHandler(Connected);
            }
            catch (Exception)
            {
                Debug.WriteLine($"Curiosity Client Queue - Connected()");
            }
        }
    }
}
