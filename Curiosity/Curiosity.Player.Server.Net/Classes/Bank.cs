﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes
{
    public class Bank
    {
        static Server server = Server.GetInstance();
        static long timerCheck = API.GetGameTimer();
        static int minutesInterest = 30;
        static int timeMark = (1000 * 60) * minutesInterest;
        static float bankInterestPct = 0.013f;

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Bank:IncreaseCash", new Action<CitizenFX.Core.Player, int, int>(IncreaseCash));
            server.RegisterEventHandler("curiosity:Server:Bank:DecreaseCash", new Action<CitizenFX.Core.Player, int, int>(DecreaseCash));
            server.RegisterEventHandler("curiosity:Server:Bank:IncreaseBank", new Action<CitizenFX.Core.Player, int, int>(IncreaseBank));
            server.RegisterEventHandler("curiosity:Server:Bank:DecreaseBank", new Action<CitizenFX.Core.Player, int, int>(DecreaseBank));
            server.RegisterEventHandler("curiosity:Server:Bank:TransferMoney", new Action<CitizenFX.Core.Player, int, bool>(TransferMoney));

            timerCheck = API.GetGameTimer();

            bankInterestPct = float.Parse(API.GetConvar("bank_interest", $"{bankInterestPct}"));
            minutesInterest = API.GetConvarInt("bank_interest_gain", minutesInterest);
            timeMark = (1000 * 60) * minutesInterest;

            Log.Verbose($"Bank Settings -> bank_interest {bankInterestPct}");
            Log.Verbose($"Bank Settings -> bank_interest_gain {minutesInterest} mins");

            server.RegisterTickHandler(BankInterest);
        }

        static async Task BankInterest()
        {
            if ((API.GetGameTimer() - timerCheck) > timeMark)
            {
                Log.Verbose("BankInterest() -> Running");

                timerCheck = API.GetGameTimer();
                Dictionary<string, Session> sessions = SessionManager.PlayerList;

                if (sessions.Count > 0)
                {
                    foreach (KeyValuePair<string, Session> keyValuePair in sessions)
                    {
                        Session session = keyValuePair.Value;
                        double interestDouble = (session.BankAccount * bankInterestPct);
                        int interest = (int)interestDouble;

                        if ((session.BankAccount + interest) > 999999999)
                        {
                            // do nothing
                        }
                        else
                        {
                            await Server.Delay(0);
                            Database.DatabaseUsersBank.IncreaseBank(session.User.BankId, interest);
                            await Server.Delay(0);
                            SessionManager.PlayerList[session.NetId].IncreaseBankAccount(interest);
                            await Server.Delay(0);
                            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
                            await Server.Delay(0);
                        }
                    }
                }
            }
            await Task.FromResult(0);
        }

        static void IncreaseCash([FromSource]CitizenFX.Core.Player player, int wallet, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (session.Wallet != wallet)
                {
                    return;
                }

                Database.DatabaseUsersBank.IncreaseCash(session.User.BankId, amount);
                session.IncreaseWallet(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
            }
            catch (Exception ex)
            {
                Log.Error($"IncreaseCash -> {ex.Message}");
            }
        }

        static void DecreaseCash([FromSource]CitizenFX.Core.Player player, int wallet, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (session.Wallet != wallet)
                {
                    return;
                }

                Database.DatabaseUsersBank.DecreaseCash(session.User.BankId, amount);
                session.DecreaseWallet(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
            }
            catch (Exception ex)
            {
                Log.Error($"DecreaseCash -> {ex.Message}");
            }
}

        static void IncreaseBank([FromSource]CitizenFX.Core.Player player, int bankAccount, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (session.BankAccount != bankAccount)
                {
                    return;
                }

                Database.DatabaseUsersBank.IncreaseBank(session.User.BankId, amount);
                session.IncreaseBankAccount(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
            }
            catch (Exception ex)
            {
                Log.Error($"IncreaseBank -> {ex.Message}");
            }
        }

        static void DecreaseBank([FromSource]CitizenFX.Core.Player player, int bankAccount, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (session.BankAccount != bankAccount)
                {
                    return;
                }

                Database.DatabaseUsersBank.DecreaseBank(session.User.BankId, amount);
                session.DecreaseBankAccount(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
            }
            catch (Exception ex)
            {
                Log.Error($"DecreaseBank -> {ex.Message}");
            }
        }

        static void TransferMoney([FromSource]CitizenFX.Core.Player player, int amount, bool toWallet)
        {
            try
            {
                if (!SessionManager.SessionExists(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (toWallet) // TODO: Improve the process
                {
                    if (amount > session.BankAccount)
                    {
                        return;
                    }

                    DecreaseBank(player, session.BankAccount, amount);
                    IncreaseCash(player, session.Wallet, amount);
                }
                else
                {
                    if (amount > session.Wallet)
                    {
                        return;
                    }

                    IncreaseBank(player, session.BankAccount, amount);
                    DecreaseCash(player, session.Wallet, amount);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"TransferMoney -> {ex.Message}");
            }
        }
    }
}
