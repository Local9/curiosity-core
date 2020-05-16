using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using System;
using CorePlayer = CitizenFX.Core.Player;

namespace Curiosity.Server.net.Classes.Menu
{
    class Player
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Player:Freeze", new Action<CorePlayer, int>(OnPlayerFreeze));
            server.RegisterEventHandler("curiosity:Server:Player:Bring", new Action<CorePlayer, int, float, float, float>(OnPlayerBring));
        }

        static void OnPlayerFreeze([FromSource]CorePlayer admin, int playerServerId)
        {
            if (!SessionManager.PlayerList.ContainsKey(admin.Handle)) return;

            if (!SessionManager.PlayerList.ContainsKey($"{playerServerId}")) return;

            Session sessionOfAdmin = SessionManager.PlayerList[admin.Handle];
            if (!sessionOfAdmin.IsStaff) return;

            Session sessionOfPlayer = SessionManager.PlayerList[$"{playerServerId}"];

            GenericData genericData = new GenericData();
            genericData.IsSentByServer = true;

            string stringToSentd = Newtonsoft.Json.JsonConvert.SerializeObject(genericData);

            sessionOfPlayer.Player.TriggerEvent("curiosity:Client:Player:Freeze", Encode.StringToBase64(stringToSentd));
        }

        static void OnPlayerBring([FromSource]CorePlayer admin, int playerServerId, float x, float y, float z)
        {
            if (!SessionManager.PlayerList.ContainsKey(admin.Handle)) return;

            if (!SessionManager.PlayerList.ContainsKey($"{playerServerId}")) return;

            Session sessionOfAdmin = SessionManager.PlayerList[admin.Handle];
            if (!sessionOfAdmin.IsStaff) return;

            Session sessionOfPlayer = SessionManager.PlayerList[$"{playerServerId}"];

            GenericData genericData = new GenericData();
            genericData.IsSentByServer = true;
            genericData.X = x;
            genericData.Y = y;
            genericData.Z = z;

            string stringToSentd = Newtonsoft.Json.JsonConvert.SerializeObject(genericData);

            sessionOfPlayer.Player.TriggerEvent("curiosity:Client:Player:Bring", Encode.StringToBase64(stringToSentd));
        }
    }
}
