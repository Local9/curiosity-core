using Curiosity.Server.net.Business;
using Curiosity.Server.net.Classes;
using Curiosity.Server.net.Entity;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
using System;

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

            Instance.ExportDictionary.Add("GetSkillUserValue", new Func<string, string, int>(
                (handle, skill) =>
                {
                    if (!SessionManager.PlayerList.ContainsKey(handle)) return -1;

                    Session session = SessionManager.PlayerList[handle];

                    if (session.Skills.ContainsKey(skill))
                    {
                        return session.Skills[skill].Value;
                    }

                    return -1;
                }
            ));

            // Func<ReturnValue>
            // Func<string, ReturnValue>
            // Func<string, int, ReturnValue>

            Instance.ExportDictionary.Add("AdjustWallet", new Func<string, int, bool, bool>(
                (handle, amount, increase) =>
                {
                    try
                    {
                        if (!SessionManager.PlayerList.ContainsKey(handle)) return false;

                        Session session = SessionManager.PlayerList[handle];

                        if (increase)
                        {
                            Database.DatabaseUsersBank.IncreaseCash(session.User.BankId, amount);
                            session.IncreaseWallet(amount);
                            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                        }
                        else
                        {
                            if (session.Wallet < amount)
                                return false;

                            Database.DatabaseUsersBank.DecreaseCash(session.User.BankId, amount);
                            session.DecreaseWallet(amount);
                            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"UpdateWallet: {ex.Message}");
                        return false;
                    }
                }
            ));

            Instance.ExportDictionary.Add("MissionComplete", new Func<string, string, bool, bool>(
                (source, missionId, passed) =>
                {
                    try
                    {
                        Mission.RecordMissionCompletion(source, missionId, passed);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"MissionComplete: {ex.Message}");

                        return false;
                    }
                }
                ));
        }
    }
}
