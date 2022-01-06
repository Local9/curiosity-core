using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Extensions;
using Curiosity.Systems.Library.Models.Casino;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.Managers
{
    public class ScriptManager : Manager<ScriptManager>
    {
        const int CASINO_INTERIOR_ID = 275201;
        bool isInsideCasino = false;

        CasinoPedScript casinoPedScript;

        public override void Begin()
        {

        }

        [TickHandler]
        private async Task OnIsPlayerInsideCasinoTick()
        {
            try
            {
                int interiorId = GetInteriorFromEntity(Game.PlayerPed.Handle);

                if (interiorId == CASINO_INTERIOR_ID && !isInsideCasino)
                {
                    isInsideCasino = true;
                    casinoPedScript = new CasinoPedScript();
                    BaseScript.RegisterScript(casinoPedScript);
                    Screen.ShowNotification($"Scrip Registered");
                }
                    

                if (interiorId != CASINO_INTERIOR_ID && isInsideCasino)
                {
                    isInsideCasino = false;
                    casinoPedScript.Dispose();
                    BaseScript.UnregisterScript(casinoPedScript);
                    casinoPedScript = null;
                    Screen.ShowNotification($"Scrip Unregistered");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"OnIsPlayerInsideCasinoTick");
            }
        }
    }
}
