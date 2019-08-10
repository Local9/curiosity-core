using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Server.net.Classes.Environment
{
    class ChatCommands
    {
        static Server server = Server.GetInstance();

        static string pilotsLoungeVideoUrl = "https//lifev.net";

        static long lastTimePIA = 0;
        static int coolDownTime = 15000;

        static string STAFF_LICENSE_PLATE = "LV0STAFF";
        static string HSTAFF_LICENSE_PLATE = "LV0HSTAF";
        static string DEV_LICENSE_PLATE = "LIFEVDEV";

        static public void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Command:SavePosition", new Action<CitizenFX.Core.Player, string, float, float, float, float>(SavePosition));

            API.RegisterCommand("chase", new Action<int, List<object>, string>(OnChaser), false);
            API.RegisterCommand("announce", new Action<int, List<object>, string>(Announcement), false);
            API.RegisterCommand("spawn", new Action<int, List<object>, string>(Spawn), false);
            API.RegisterCommand("sc", new Action<int, List<object>, string>(SpawnCar), false);
            API.RegisterCommand("dc", new Action<int, List<object>, string>(DevCar), false);
            API.RegisterCommand("deluxo", new Action<int, List<object>, string>(StaffCar), false);
            API.RegisterCommand("video", new Action<int, List<object>, string>(ChangeVideoURL), false);
            API.RegisterCommand("watch", new Action<int, List<object>, string>(RequestURL), false);

            API.RegisterCommand("pia", new Action<int, List<object>, string>(PIA), false);
        }

        static void OnChaser(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            if (arguments.Count < 0)
            {
                Helpers.Notifications.Advanced($"Agruments Missing", $"", 2, session.Player);
                return;
            }

            int playerToChase = 0;
            int.TryParse($"{arguments[0]}", out playerToChase);

            if (!SessionManager.PlayerList.ContainsKey($"{playerToChase}"))
            {
                Helpers.Notifications.Advanced($"Player Missing", $"", 2, session.Player);
                return;
            }

            Session sessionToChase = SessionManager.PlayerList[$"{playerToChase}"];
            sessionToChase.Player.TriggerEvent("curiosity:Client:Command:SpawnChaser");

            Helpers.Notifications.Advanced($"Set Chaser", $"Chasing ~y~{sessionToChase.Player.Name}", 20, session.Player);
        }

        static void PIA(int playerHandle, List<object> arguments, string raw)
        {
            if ((API.GetGameTimer() - lastTimePIA) < coolDownTime) return;

            lastTimePIA = API.GetGameTimer();

            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            Server.TriggerClientEvent("curiosity:Client:Chat:Message", "PIA", "dodgerblue", "Q: <- <u>I cannot see PIA or SSIA. What do I do to fix it?</u> -><br /><i>In advanced Graphics in your settings</i><br />1.<b>Turning High Detail Streaming While Flying</b>[ON]<br />2.<b>Extended - Distance Scaling to max</b> (or high depending on your PC<br />3.Restart your FiveM<hr /><img src=https://i.imgur.com/KnXKWhL.jpg width=100% />");
        }

        static void RequestURL(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            session.Player.TriggerEvent("curiosity:Client:Video:SetUrl", pilotsLoungeVideoUrl);
        }

        static void ChangeVideoURL(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

                Session session = SessionManager.PlayerList[$"{playerHandle}"];

                if (!session.IsDeveloper) return;

                if ($"{arguments[0]}" == pilotsLoungeVideoUrl) return;

                pilotsLoungeVideoUrl = $@"{arguments[0]}";

                Uri myUri = new Uri(pilotsLoungeVideoUrl);

                if (myUri.Authority.Contains("youtube"))
                {
                    if (!pilotsLoungeVideoUrl.Contains("embed"))
                    {
                        var parameters = HttpUtility.ParseQueryString(myUri.Query);
                        pilotsLoungeVideoUrl = $@"https://www.youtube.com/embed/{parameters["v"]}?autoplay=1&rel=0&controls=0&disablekb=1";
                    }
                }

                Server.TriggerClientEvent("curiosity:Client:Video:SetUrl", pilotsLoungeVideoUrl);
            }
            catch (Exception ex)
            {
                Log.Warn("ChangeVideoURL() -> I think you forgot to wrap quotes around the url...");
            }
        }

        static void SavePosition([FromSource]CitizenFX.Core.Player player, string positionName, float posX, float posY, float posZ, float heading)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (!session.IsDeveloper) return;

                string filePath = $@"{API.GetResourcePath(API.GetCurrentResourceName())}\data\{session.Name}-savedPositions.txt";

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();

                    using (TextWriter tw = new StreamWriter(filePath))
                    {
                        tw.WriteLine("Date,Name,X,Y,Z,Heading");
                    }
                }

                using (TextWriter tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{positionName},{posX},{posY},{posZ},{heading}");
                }

                Helpers.Notifications.Advanced($"Position Saved", $"~b~Name: ~s~{positionName}", 20, session.Player);
            }
            catch (Exception ex)
            {
                Log.Error($"SaveCoords() -> {ex.Message}");
            }
        }

        static void Announcement(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (session.Privilege == Privilege.USER) return;

            List<string> args = arguments.Cast<string>().ToList();
            Server.TriggerClientEvent("curiosity:Client:Scalefrom:Announce", String.Join(" ", args));
        }

        static void SpawnCar(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

                if (arguments.Count < 1) return;

                Session session = SessionManager.PlayerList[$"{playerHandle}"];

                if (!session.IsDeveloper) return;

                session.Player.TriggerEvent("curiosity:Client:Command:SpawnCar", arguments[0], HSTAFF_LICENSE_PLATE);
            }
            catch (Exception ex)
            {
                Log.Error($"Chat->Spawn-> {ex.Message}");
            }
        }

        static void StaffCar(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;
                Session session = SessionManager.PlayerList[$"{playerHandle}"];

                if (!session.IsStaff) return;

                session.Player.TriggerEvent("curiosity:Client:Command:SpawnCar", "deluxo", STAFF_LICENSE_PLATE);
            }
            catch (Exception ex)
            {
                Log.Error($"Chat->Spawn-> {ex.Message}");
            }
        }

        static void DevCar(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;
                Session session = SessionManager.PlayerList[$"{playerHandle}"];
                if (session.Privilege != Privilege.DEVELOPER) return;

                string carName = "thrax";

                if (arguments.Count > 0)
                    carName = $"{arguments[0]}";

                session.Player.TriggerEvent("curiosity:Client:Command:SpawnCar", carName, DEV_LICENSE_PLATE);
            }
            catch (Exception ex)
            {
                Log.Error($"Chat->Spawn-> {ex.Message}");
            }
        }

        static void Spawn(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

                Session session = SessionManager.PlayerList[$"{playerHandle}"];

                if (!session.IsDeveloper) return;

                List<string> args = arguments.Cast<string>().ToList();
                string type = args[0];

                switch (type)
                {
                    case "weapon":
                        session.Player.TriggerEvent("curiosity:Client:Command:SpawnWeapon", args[1]);
                        break;
                    case "car":
                        session.Player.TriggerEvent("curiosity:Client:Command:SpawnCar", args[1]);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Chat->Spawn-> {ex.Message}");
            }
        }
    }
}
