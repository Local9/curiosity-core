using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Server.net.Extensions;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Curiosity.Server.net.Classes.Environment
{
    class ChatService
    {
        static Server server = Server.GetInstance();
        static Regex regex = new Regex(@"^[\x20-\x7E£]+$");

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Chat:Message", new Action<CitizenFX.Core.Player, string, string>(ProcessMessage));

            // TODO : MOVE THESE
            server.RegisterEventHandler("entityCreated", new Action<dynamic>(OnEntityCreated));
            // server.RegisterEventHandler("entityCreating", new Action<dynamic>(OnEntityCreating));
            server.RegisterEventHandler("explosionEvent", new Action<int, dynamic>(OnExplosionEvent));
        }

        static void OnExplosionEvent(int sender, dynamic explosionData) // TODO : MOVE
        {
            try
            {
                List<Session> sessions = SessionManager.PlayerList.Select(m => m.Value).Where(w => w.IsStaff).ToList();
                Session senderSession = SessionManager.PlayerList[$"{sender}"];

                if (explosionData.ownerNetId == 0) return;

                Session explosionSession = SessionManager.PlayerList[$"{explosionData.ownerNetId}"];

                double secondsSince = (DateTime.Now - senderSession.LastEntityEvent).TotalSeconds;

                ExplosionTypes explosionType = (ExplosionTypes)explosionData.explosionType;

                if (secondsSince > 3)
                {
                    senderSession.SetLastExplosionEvent();
                    return;
                }
                senderSession.SetLastExplosionEvent();

                sessions.ForEach(session =>
                {
                    ChatLog.SendLogMessage($"Event Sender: {senderSession.Name},\n\rExplosion Owner: {explosionSession.Name} - {explosionType}", session.Player);
                });
            }
            catch (Exception ex)
            {
                Log.Verbose($"OnExplosionEvent -> {ex.Message}");
            }
        }

        static void OnEntityCreating(int entity) // TODO : MOVE
        {
            List<Session> sessions = SessionManager.PlayerList.Select(m => m.Value).Where(w => w.IsStaff).ToList();
            sessions.ForEach(session =>
            {

                ChatLog.SendLogMessage($"CREATING: {JsonConvert.SerializeObject(entity)}");
            });
        }

        static void OnEntityCreated(dynamic entity) // TODO : MOVE
        {
            try
            {
                List<Session> sessions = SessionManager.PlayerList.Select(m => m.Value).Where(w => w.IsStaff).ToList();

                int entityId = 0;

                if (!int.TryParse($"{entity}", out entityId)) return;

                if (!API.DoesEntityExist(entityId)) return;

                int entityType = API.GetEntityType(entityId);

                if (entityType == 0 || entityType == 1) return; // Ignore no entity, ped

                Vehicle vehicle = null;

                if (entityType == 2)
                {
                    vehicle = new Vehicle(entityId);
                }

                if (vehicle == null)
                {
                    return;
                }

                CitizenFX.Core.Entity entityObject = vehicle;

                int populationTypeId = API.GetEntityPopulationType(entityId);

                if (populationTypeId == 1 || populationTypeId == 2 || populationTypeId == 3 || populationTypeId == 4 || populationTypeId == 5) return; // Ignore random population

                Session entityOwnerSession = SessionManager.PlayerList[entityObject.Owner.Handle];

                if (entityOwnerSession.IsStaff) return; // IGNORE STAFF

                double secondsSince = (DateTime.Now - entityOwnerSession.LastEntityEvent).TotalSeconds;

                if (secondsSince > 3)
                {
                    entityOwnerSession.SetLastEntityEvent();
                    return;
                }
                entityOwnerSession.SetLastEntityEvent();

                sessions.ForEach(session =>
                {
                    ChatLog.SendLogMessage($"{entityOwnerSession.Name} - Is creating entities very quickly");
                });
            }
            catch (Exception ex)
            {
                Log.Verbose($"OnEntityCreated -> {ex.Message}");
            }
        }

        static void ProcessMessage([FromSource]CitizenFX.Core.Player player, string message, string chatChannel)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                if (!Server.isLive)
                {
                    Log.Verbose($"[ProcessMessage] {message}");
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    API.CancelEvent();
                    return;
                }

                if (!regex.Match(message).Success)
                {
                    API.CancelEvent();
                    return;
                }

                if (message.Length == 0 || message.Length > 240)
                {
                    message = message.Substring(0, 240);
                }

                if (message.ContainsProfanity())
                {
                    Regex wordFilter = new Regex($"({string.Join("|", ProfanityFilter.ProfanityArray())})");
                    message = wordFilter.Replace(message, "$!\"£^!@");
                }

                if (message.IsAllUpper())
                {
                    message.ToLower();
                }

                ChatMessage chatMessage = new ChatMessage();

                chatMessage.color = $"{session.Privilege}".ToLower();
                chatMessage.role = $"{session.Privilege}";
                chatMessage.list = chatChannel;
                chatMessage.message = message;
                chatMessage.roleClass = $"{session.Privilege}";
                chatMessage.name = $"[{session.Player.Handle}] {session.Player.Name}";
                chatMessage.job = $"{session.job}";

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage);
                string encoded = Encode.StringToBase64(json);

                Log.Verbose($"[{player.Handle}] {player.Name}#{session.UserID} - {message}");

                DiscordWrapper.SendDiscordChatMessage($"[{player.Handle}] {player.Name}#{session.UserID}", message);

                Server.TriggerClientEvent("curiosity:Client:Chat:Message", encoded);
            }
            catch (Exception ex)
            {
                Log.Error($"ProcessMessage -> {ex.Message}");
            }
        }
    }
}
