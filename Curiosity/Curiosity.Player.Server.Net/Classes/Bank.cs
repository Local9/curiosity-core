using CitizenFX.Core;
using System;

namespace Curiosity.Server.net.Classes
{
    public class Bank
    {
        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseCash", new Action<Player, int, int>(IncreaseCash));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseCash", new Action<Player, int, int>(DecreaseCash));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseBank", new Action<Player, int, int>(IncreaseBank));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseBank", new Action<Player, int, int>(DecreaseBank));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:TransferMoney", new Action<Player, int, bool>(TransferMoney));
        }

        static void IncreaseCash([FromSource]Player player, int wallet, int amount)
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

        static void DecreaseCash([FromSource]Player player, int wallet, int amount)
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

        static void IncreaseBank([FromSource]Player player, int bankAccount, int amount)
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

        static void DecreaseBank([FromSource]Player player, int bankAccount, int amount)
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

        static void TransferMoney([FromSource]Player player, int amount, bool toWallet)
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
