using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes.Environment
{
    class InstanceChecker
    {
        static long gameTimer = API.GetGameTimer();
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterTickHandler(StartInstanceChecker);
        }

        static async Task StartInstanceChecker()
        {
            if ((API.GetGameTimer() - gameTimer) > 60000)
            {
                gameTimer = API.GetGameTimer();
                BaseScript.TriggerClientEvent("curiosity:Client:Settings:PlayerCount", Server.players.Count());
            }
            await Task.FromResult(0);
        }
    }
}
