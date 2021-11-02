using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Environment.Entities.Models;
using Curiosity.Police.Client.Environment.Entities.Modules.Impl;
using Curiosity.Police.Client.Events;
using Curiosity.Police.Client.Managers;
using System.Threading.Tasks;

namespace Curiosity.Police.Client
{
    public class Session
    {
        public static int LastSession { get; set; }

        static bool setup = false;

        public static async Task Loading()
        {
            if (!setup)
                await BaseScript.Delay(3000);
        }
    }
}