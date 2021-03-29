using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerStatManager : Manager<PlayerStatManager>
    {
        bool wasSprinting = false;
        bool wasSwimming = false;

        DateTime DEFAULT = new DateTime(1970, 01, 01);

        DateTime sprintStart = new DateTime(1970, 01, 01);
        DateTime sprintEnd = new DateTime(1970, 01, 01);

        DateTime swimStart = new DateTime(1970, 01, 01);
        DateTime swimEnd = new DateTime(1970, 01, 01);

        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async Task OnPlayerStateTask()
        {
            if (Cache.PlayerPed.IsSprinting && !wasSprinting)
            {
                wasSprinting = true;
                sprintStart = DateTime.Now;
            }
            else if (!Cache.PlayerPed.IsSprinting && wasSprinting)
            {
                wasSprinting = false;
                sprintEnd = DateTime.Now;
                // Log Duration
                double sec = sprintEnd.Subtract(sprintStart).TotalSeconds;
                Logger.Debug($"Duration Sprinted: {sec:0.00}");
                // Send data back, update server, get total and find level with total, this then = Stamina Stat

                // Reset
                sprintStart = DEFAULT;
                sprintEnd = DEFAULT;
            }

            if (Cache.PlayerPed.IsSwimming && !wasSwimming)
            {
                wasSwimming = true;
                swimStart = DateTime.Now;
            }
            else if (!Cache.PlayerPed.IsSwimming && wasSwimming)
            {
                wasSwimming = false;
                swimEnd = DateTime.Now;

                double sec = swimEnd.Subtract(swimStart).TotalSeconds;
                Logger.Debug($"Duration Swam: {sec:0.00}");
                // Send data back, update server, get total and find level with total, this then = Breathing Stat

                swimStart = DEFAULT;
                swimEnd = DEFAULT;
            }
        }
    }
}
