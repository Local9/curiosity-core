﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

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
            server.RegisterEventHandler("curiosity:Server:Command:NukeArea", new Action<CitizenFX.Core.Player, float, float, float>(OnNuke));

            API.RegisterCommand("donator", new Action<int, List<object>, string>(OnDonationCheck), false);
            API.RegisterCommand("revive", new Action<int, List<object>, string>(OnRevivePlayer), false);

            API.RegisterCommand("chase", new Action<int, List<object>, string>(OnChaser), false);
            API.RegisterCommand("chase2", new Action<int, List<object>, string>(OnChaserTwo), false);
            API.RegisterCommand("announce", new Action<int, List<object>, string>(Announcement), false);
            API.RegisterCommand("spawn", new Action<int, List<object>, string>(Spawn), false);
            API.RegisterCommand("sc", new Action<int, List<object>, string>(SpawnCar), false);
            API.RegisterCommand("dc", new Action<int, List<object>, string>(DevCar), false);
            API.RegisterCommand("deluxo", new Action<int, List<object>, string>(StaffCar), false);
            API.RegisterCommand("video", new Action<int, List<object>, string>(ChangeVideoURL), false);
            API.RegisterCommand("watch", new Action<int, List<object>, string>(RequestURL), false);

            API.RegisterCommand("chimp", new Action<int, List<object>, string>(SpawnChimp), false);

            // API.RegisterCommand("pia", new Action<int, List<object>, string>(PIA), false);

            API.RegisterCommand("test", new Action<int, List<object>, string>(Test), false);
            API.RegisterCommand("sessions", new Action<int, List<object>, string>(OnSessions), false);
            API.RegisterCommand("guide", new Action<int, List<object>, string>(Guide), false);

            server.RegisterEventHandler("rconCommand", new Action<string, List<object>>(OnRconCommand));

            // API.RegisterCommand("onfire", new Action<int, List<object>, string>(OnFire), false);
        }

        //static void OnFire(int playerHandle, List<object> arguments, string raw)
        //{
        //    if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

        //    Session session = SessionManager.PlayerList[$"{playerHandle}"];

        //    if (!session.IsDeveloper) return;

        //    Server.TriggerClientEvent("curiosity:Client:Command:OnFire", session.NetId);
        //}

        static void OnRconCommand(string commandName, List<object> arguments)
        {
            Log.Verbose($"[SERVER] Running command: {commandName}");

            if (commandName.ToLower() == "session")
            {

                int sessionCount = SessionManager.PlayerList.Count;
                int playerCount = Server.players.Count();

                Log.Info($"[SERVER] Active Sessions: {sessionCount}, Player Count: {playerCount}");
            }

            if (commandName.ToLower() == "announcement")
            {
                List<string> message = arguments.Cast<string>().ToList();
                string messageToSend = String.Join(" ", message);

                Log.Info($"[SERVER] Announcing: {messageToSend}");

                Server.TriggerClientEvent("curiosity:Client:Scalefrom:Announce", messageToSend);
            }

            try
            {
                API.CancelEvent();
            }
            catch (Exception ex)
            {
                Log.Error($"Error when canceling event, possible changes made by FiveM Collective.");
            }
        }

        static void OnSessions(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsDeveloper) return;

            int sessionCount = SessionManager.PlayerList.Count;
            int playerCount = Server.players.Count();

            Helpers.Notifications.Advanced($"Session Check", $"Active Sessions: {sessionCount}~n~Player Count: {playerCount}", 2, session.Player);
        }
        static void OnRevivePlayer(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (!session.IsStaff) return;

            if (arguments.Count == 0) return;

            string playerToRevive = $"{arguments.ElementAt(0)}";
            Session sessionToRevive = SessionManager.PlayerList[$"{playerToRevive}"];
            sessionToRevive.Player.TriggerEvent("curiosity:Client:Player:Revive");
        }

        static void OnDonationCheck(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if ((DateTime.Now - session.LastDonationCheck).TotalMinutes < 30)
            {
                Helpers.Notifications.Advanced($"Donation Check", $"You tried checking in the last 30 mins, please try again later.", 2, session.Player);
            }
            else
            {
                Classes.Character.CharacterRoleCheck(session.Player);
            }
        }

        static void SpawnChimp(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (session.Privilege != Privilege.DEVELOPER) return;

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
            sessionToChase.Player.TriggerEvent("curiosity:Client:Command:Chimp");

            Helpers.Notifications.Advanced($"Set Chimp", $"Shooting ~y~{sessionToChase.Player.Name}", 20, session.Player);
        }

        static void OnChaserTwo(int playerHandle, List<object> arguments, string raw)
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

            sessionToChase.Player.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");

            Helpers.Notifications.Advanced($"Set Chaser", $"Chasing ~y~{sessionToChase.Player.Name}", 20, session.Player);
            Helpers.Notifications.Advanced($"Server System Activated", $"I recommend you leave...", 20, sessionToChase.Player);
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

            if (sessionToChase.IsDeveloper)
            {
                session.Player.TriggerEvent("curiosity:Client:Player:UpdateFlags");
                Helpers.Notifications.Advanced($"You fucked up...", $"Cannot chase a developer", 20, session.Player);
            }
            else
            {
                sessionToChase.Player.TriggerEvent("curiosity:Client:Player:UpdateFlags");
            }

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

        static void Guide(int playerHandle, List<object> arguments, string raw)
        {
            if ((API.GetGameTimer() - lastTimePIA) < coolDownTime) return;

            lastTimePIA = API.GetGameTimer();

            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            ChatMessage chatMessage = new ChatMessage();

            chatMessage.color = $"{Privilege.DEVELOPER}".ToLower();
            chatMessage.role = $"{Privilege.DEVELOPER}";
            chatMessage.list = "chat";
            chatMessage.message = "See out our guide '[ELV] Welcome to Emergency Life V Player - Guide(READ ME)' which can be found on our forums @ forums.lifev.net or ingame via M -> Open Player-Guide. Starting jobs are EMT/Fire/Police.";
            chatMessage.roleClass = $"{Privilege.DEVELOPER}";
            chatMessage.name = "GUIDE";
            chatMessage.job = $"Guide";

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage);
            string encoded = Encode.StringToBase64(json);

            session.Player.TriggerEvent("curiosity:Client:Chat:Message", encoded);
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
                        tw.WriteLine("Date,Name,X,Y,Z,Heading,Vector,VectorJson,VectorLua");
                    }
                }

                using (TextWriter tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{positionName},{posX},{posY},{posZ},{heading},({posX}f, {posY}f, {posZ}f),(x: {posX}, y: {posY}, z: {posZ}),({posX}, {posY}, {posZ})");
                }

                Helpers.Notifications.Advanced($"Position Saved", $"~b~Name: ~s~{positionName}", 20, session.Player);
            }
            catch (Exception ex)
            {
                Log.Error($"SaveCoords() -> {ex.Message}");
            }
        }

        static void OnNuke([FromSource]CitizenFX.Core.Player player, float x, float y, float z)
        {
            Session session = SessionManager.PlayerList[$"{player.Handle}"];

            if (session.Privilege == Privilege.USER) return;

            Server.TriggerClientEvent("curiosity:Client:Command:Nuke", x, y, z);
        }

        static void Announcement(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (session.Privilege == Privilege.USER) return;

            List<string> args = arguments.Cast<string>().ToList();
            Server.TriggerClientEvent("curiosity:Client:Scalefrom:Announce", String.Join(" ", args));
        }

        static void Test(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (session.Privilege == Privilege.USER) return;

            List<string> args = arguments.Cast<string>().ToList();
            Server.TriggerClientEvent("curiosity:Client:Scalefrom:MissionComplete", String.Join(" ", args));
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
