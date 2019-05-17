using CitizenFX.Core;
using System;

namespace Curiosity.Server.net.Classes
{
    public class Bank
    {
        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseCash", new Action<Player, int>(IncreaseCash));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseCash", new Action<Player, int>(DecreaseCash));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:IncreaseBank", new Action<Player, int>(IncreaseBank));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:DecreaseBank", new Action<Player, int>(DecreaseBank));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Bank:TransferMoney", new Action<Player, int, bool>(TransferMoney));
        }

        static void IncreaseCash([FromSource]Player player, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];
                Database.DatabaseUsersBank.IncreaseCash(session.UserID, amount);
                session.IncreaseWallet(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseCash -> {ex.Message}");
            }
        }

        static void DecreaseCash([FromSource]Player player, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];
                Database.DatabaseUsersBank.DecreaseCash(session.UserID, amount);
                session.DecreaseWallet(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecreaseCash -> {ex.Message}");
            }
}

        static void IncreaseBank([FromSource]Player player, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];
                Database.DatabaseUsersBank.IncreaseBank(session.UserID, amount);
                session.IncreaseBankAccount(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseBank -> {ex.Message}");
            }
        }

        static void DecreaseBank([FromSource]Player player, int amount)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];
                Database.DatabaseUsersBank.DecreaseBank(session.UserID, amount);
                session.DecreaseBankAccount(amount);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecreaseBank -> {ex.Message}");
            }
        }

        static void TransferMoney([FromSource]Player player, int amount, bool toWallet)
        {
            try
            {
                if (toWallet) // TODO: Improve the process
                {
                    DecreaseBank(player, amount);
                    IncreaseCash(player, amount);
                }
                else
                {
                    IncreaseBank(player, amount);
                    DecreaseCash(player, amount);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecreaseBank -> {ex.Message}");
            }
        }
    }
}
