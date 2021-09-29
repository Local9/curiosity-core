using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Racing.Client.Environment.Entities.Models;
using Curiosity.Racing.Client.Environment.Entities.Modules.Impl;
using Curiosity.Racing.Client.Events;
using Curiosity.Racing.Client.Managers;
using System.Threading.Tasks;

namespace Curiosity.Racing.Client
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