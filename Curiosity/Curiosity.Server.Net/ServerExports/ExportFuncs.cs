using CitizenFX.Core;
using Curiosity.Server.net.Classes;
using Curiosity.Server.net.Entity;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
using System;
using System.Dynamic;

namespace Curiosity.Server.net.ServerExports
{
    public class ExportFuncs
    {

        private static Server Instance = Server.GetInstance();

        public static void Init()
        {
            Instance.ExportDictionary.Add("Status", new Func<bool>(
                () =>
                {
                    return Server.serverActive;
                }
            ));

            Instance.ExportDictionary.Add("GetUser", new Func<string, string>(
                (handle) =>
                {
                    if (!SessionManager.PlayerList.ContainsKey(handle)) return null;

                    Session session = SessionManager.PlayerList[handle];

                    CuriosityUser curUser = new CuriosityUser();

                    curUser.UserId = session.UserID;
                    curUser.LatestName = session.Name;
                    curUser.Role = session.Privilege;

                    return JsonConvert.SerializeObject(curUser);
                }
            ));

            // Func<ReturnValue>
            // Func<string, ReturnValue>
            // Func<string, int, ReturnValue>

            Instance.ExportDictionary.Add("UpdateWallet", new Func<string, int, bool>(
                (handle, amount) =>
                {
                    if (!SessionManager.PlayerList.ContainsKey(handle)) return false;

                    Session session = SessionManager.PlayerList[handle];

                    if (amount > 0)
                    {
                        Database.DatabaseUsersBank.IncreaseCash(session.User.BankId, amount);
                        session.IncreaseWallet(amount);
                        session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                    }
                    else
                    {
                        Database.DatabaseUsersBank.DecreaseCash(session.User.BankId, amount);
                        session.DecreaseWallet(amount);
                        session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                    }

                    return true;
                }
            ));
        }
    }
}
