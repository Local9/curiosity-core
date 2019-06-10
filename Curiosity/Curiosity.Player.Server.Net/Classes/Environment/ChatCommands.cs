﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Curiosity.Server.net.Classes.Environment
{
    class ChatCommands
    {
        static Server server = Server.GetInstance();

        static string pilotsLoungeVideoUrl = "https//lifev.net";

        static public void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Command:SavePosition", new Action<CitizenFX.Core.Player, string, float, float, float>(SavePosition));

            API.RegisterCommand("announce", new Action<int, List<object>, string>(Announcement), false);
            API.RegisterCommand("spawn", new Action<int, List<object>, string>(Spawn), false);
            API.RegisterCommand("sc", new Action<int, List<object>, string>(SpawnCar), false);
            API.RegisterCommand("video", new Action<int, List<object>, string>(ChangeVideoURL), false);
            API.RegisterCommand("watch", new Action<int, List<object>, string>(RequestURL), false);
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

                session.Player.TriggerEvent("curiosity:Client:Video:SetUrl", pilotsLoungeVideoUrl);
            }
            catch (Exception ex)
            {
                Log.Warn("ChangeVideoURL() -> I think you forgot to wrap quotes around the url...");
            }
        }

        static void SavePosition([FromSource]CitizenFX.Core.Player player, string positionName, float posX, float posY, float posZ)
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
                        tw.WriteLine("Date,Name,X,Y,Z");
                    }
                }

                using (TextWriter tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{positionName},{posX},{posY},{posZ}");
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

                Session session = SessionManager.PlayerList[$"{playerHandle}"];

                if (!session.IsDeveloper) return;

                session.Player.TriggerEvent("curiosity:Client:Command:SpawnCar", arguments[0]);
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
