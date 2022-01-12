using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Core.Client.Utils;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class JailManager : Manager<JailManager>
    {
        Vector3 jailStart = new Vector3(1849.58679f, 2605.6687f, 40.5720177f);
        Vector3 jailEnd = new Vector3(1522.98364f, 2598.043f, 90.95463f);
        float width = 400;

        public bool IsJailed = false;

        public override void Begin()
        {
            
        }

        
        private async Task OnJailCheck()
        {
            bool isInsideJail = Common.IsEntityInAngledArea(Game.PlayerPed, jailStart, jailEnd, width);
            if (isInsideJail) return; // if they are jailed and still inside, do nothing
            
            // if they have left, then we need to inform the police
        }
    }
}
