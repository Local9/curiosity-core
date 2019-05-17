﻿using System;

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
    static class Bank
    {
        static Client client = Client.GetInstance();

        public static int wallet = 0;
        public static int bank = 0;
        
        const int HUD_SCALEFORM_ID = 4;
        static bool showWallet = false;

        static int bankBalanceHashKey;
        static int cashBalanceHashKey;

        static public void Init()
        {
            BaseScript.Delay(5000);
            client.RegisterTickHandler(SetupBank);
            client.RegisterTickHandler(SetupWallet);
            client.RegisterTickHandler(TickCash);
            client.RegisterTickHandler(ShowWallet);

            client.RegisterEventHandler("curiosity:Client:Bank:UpdateWallet", new Action<int>(UpdateWallet));
            client.RegisterEventHandler("curiosity:Client:Bank:UpdateBank", new Action<int>(UpdateBank));

            bankBalanceHashKey = GetHashKey("BANK_BALANCE");
            cashBalanceHashKey = GetHashKey("MP0_WALLET_BALANCE");
            BaseScript.Delay(0);
            //int stam = GetHashKey("MP0_STAMINA");
            //StatSetInt((uint)stam, 55, true);
            //BaseScript.Delay(0);
            //int shoot = GetHashKey("MP0_SHOOTING_ABILITY");
            //StatSetInt((uint)shoot, 25, true);
        }

        static async void UpdateWallet(int amount)
        {
            wallet = amount;
            await BaseScript.Delay(0);
        }

        static async void UpdateBank(int amount)
        {
            bank = amount;
            await BaseScript.Delay(0);
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
                if (!IsHudComponentActive(HUD_SCALEFORM_ID))
                    ShowHudComponentThisFrame(HUD_SCALEFORM_ID);
            }
            await Task.FromResult(0);
        }

        static async void HideWallet()
        {
            await BaseScript.Delay(3000);
            showWallet = false;
            DisplayCash(false);
        }
    }
}
