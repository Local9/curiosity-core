using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class Cash
    {
        static int wallet = 100;
        static int bank = 2000;

        static bool loaded = false;
        const int HUD_SCALEFORM_ID = 3;
        static bool showWallet = false;

        static int bankBalanceHashKey;
        static int cashBalanceHashKey;

        static public void Init()
        {
            BaseScript.Delay(5000);
            Client.GetInstance().RegisterTickHandler(SetupBank);
            Client.GetInstance().RegisterTickHandler(SetupWallet);
            Client.GetInstance().RegisterTickHandler(TickCash);
            Client.GetInstance().RegisterTickHandler(ShowWallet);

            bankBalanceHashKey = GetHashKey("BANK_BALANCE");
            cashBalanceHashKey = GetHashKey("MP0_WALLET_BALANCE");
        }

        static async Task SetupBank()
        {
            StatSetInt((uint)bankBalanceHashKey, bank, true);
            await Task.FromResult(0);            
        }

        static async Task SetupWallet()
        {

            StatSetInt((uint)cashBalanceHashKey, wallet, true);
            await Task.FromResult(0);
        }

        static async Task TickCash()
        {
            if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
            {
                DisplayCash(true);
                showWallet = true;
                HideWallet();
            }
            await Task.FromResult(0);
        }

        static async Task ShowWallet()
        {
            while (showWallet)
            {
                await BaseScript.Delay(0);
                if (!IsHudComponentActive(4))
                    ShowHudComponentThisFrame(4);
            }
            await Task.FromResult(0);
        }

        static async void HideWallet()
        {
            await BaseScript.Delay(3000);
            showWallet = false;
        }
    }
}
