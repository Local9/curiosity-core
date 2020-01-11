using System;
using System.Collections.Generic;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Client.Package;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Atlas.Roleplay.Client.Login
{
    public class LoginManager : Manager<LoginManager>
    {
        public override async void Begin()
        {
            API.NetworkClearVoiceChannel();
            API.NetworkSetTalkerProximity(0.1f);
            API.NetworkSetFriendlyFireOption(true);
            API.SetCanAttackFriendly(API.PlayerPedId(), true, true);
            API.StopPlayerSwitch();

            var sound = new SoundSystem();
            var handle = API.GetPlayerServerId(API.PlayerId());

            sound.Disable();

            await Game.Player.ChangeModel(new Model(API.GetHashKey("mp_m_freemode_01")));

            var ped = Game.PlayerPed.Handle;

            API.RemoveAllPedWeapons(ped, false);
            API.ClearPedTasksImmediately(ped);
            API.ClearPlayerWantedLevel(API.PlayerId());
            API.SetEntityVisible(ped, true, true);
            API.SetEntityHealth(ped, API.GetEntityMaxHealth(ped));
            API.NetworkResurrectLocalPlayer(0, 0, 70f, 0f, true, false);
            API.FreezeEntityPosition(ped, false);
            API.SendLoadingScreenMessage(new JsonBuilder().Add("eventName", "UPDATE_STATUS").Add("status", "Laddar in användare").Build());

            var transition = new CharacterManager.LoadTransition();
            var user = await EventSystem.Request<AtlasUser>("user:login");
            var payloads = new Dictionary<string, string>();

            try
            {
                payloads = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    (await EventSystem.Request<object>("network:package:latest")).ToString());
            }
            catch (Exception)
            {
                // Ignored
            }

            var package = NetworkPackage.GetModule();

            foreach (var payload in payloads)
            {
                package.Payloads[payload.Key] = JsonConvert.DeserializeObject<NetworkPayload<object>>(payload.Value);
            }

            Logger.Info($"[User] [{user.Seed}] Creating local player...");

            Atlas.Local = new AtlasPlayer(user.SteamId, new AtlasEntity(ped))
            {
                Handle = handle,
                Name = user.LastName,
                User = user
            };

            var voice = VoiceChat.GetModule();

            voice.Range = 0f;
            voice.Commit();

            Atlas.AttachTickHandlers(voice);
            Atlas.Local.DisableHud();

            Logger.Info(
                $"[User] [{user.Seed}] Logged into the server with the role `{user.Role.ToString()}` and metadata: {JsonConvert.SerializeObject(user.Metadata)}");

#pragma warning disable 4014
            transition.Up(Atlas.Local);
#pragma warning restore 4014

            // Synchronize characters with the server-side, that is fetching from the database.
            await CharacterManager.GetModule().Synchronize();
        }
    }
}