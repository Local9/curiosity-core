﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.net.Extensions;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public CuriosityCharacter curiosityCharacter = new CuriosityCharacter();

        public override void Begin()
        {
            
        }

        public class LoadTransition
        {
            public PluginManager Curiosity { get; set; }

            public LoadTransition()
            {
                Curiosity = PluginManager.Instance;
            }

            public async Task Up(CuriosityPlayer player)
            {
                var timestamp = API.GetGameTimer();

                Curiosity.AttachTickHandler(OnTick);

                player.Sound.Disable();

                API.SwitchOutPlayer(player.Entity.Id, 0, 1);

                while (API.GetPlayerSwitchState() != 5 && timestamp + 15000 > API.GetGameTimer())
                {
                    await BaseScript.Delay(0);
                }

                player.Sound.Enable();

                Curiosity.DetachTickHandler(OnTick);
            }

            public async Task Wait()
            {
                while (API.GetPlayerSwitchState() < 3)
                {
                    await BaseScript.Delay(100);
                }
            }

            public async Task Down(CuriosityPlayer player)
            {
                while (API.GetPlayerSwitchState() != 5)
                {
                    await BaseScript.Delay(100);
                }

                Curiosity.AttachTickHandler(OnTick);

                var timestamp = API.GetGameTimer();

                while (timestamp + 3000 > API.GetGameTimer())
                {
                    await BaseScript.Delay(10);
                }

                API.SwitchInPlayer(player.Entity.Id);

                while (API.GetPlayerSwitchState() != 12)
                {
                    await BaseScript.Delay(10);
                }

                Curiosity.DetachTickHandler(OnTick);

                API.ClearDrawOrigin();
            }

            public async Task DownWait()
            {
                if (Cache.Character.MarkedAsRegistered)
                {
                    while (API.GetPlayerSwitchState() < 10)
                    {
                        await BaseScript.Delay(10);
                    }
                }
                else
                {
                    while (Cache.Character.MarkedAsRegistered && !API.IsScreenFadedOut())
                    {
                        await BaseScript.Delay(10);
                    }
                }
            }

            private async Task OnTick()
            {
                API.SetCloudHatOpacity(0.05f);
                API.HideHudAndRadarThisFrame();

                await Task.FromResult(0);
            }
        }

        public async Task Synchronize()
        {
            Screen.Fading.FadeIn(0);

            Instance.ExportDictionary["pma-voice"].toggleMute();

            API.SetTimecycleModifier("default");
            API.SetTimecycleModifierStrength(1f);

            Logger.Debug("[Character] Loading character data...");

            Screen.LoadingPrompt.Show("Loading Character Data...");

            curiosityCharacter = await EventSystem.Request<CuriosityCharacter>("character:load");

            if (curiosityCharacter == null)
            {
                Logger.Error("[Character] No character information returned");
                Screen.LoadingPrompt.Show("[ERROR] Press F8 and screenshot the console ERROR");
                return;
            }

            Logger.Debug("[Character] Loaded character data...");

            Instance.Local.Character = curiosityCharacter;

            Screen.LoadingPrompt.Show("Loading Character...");

            await Load(Instance.Local);

            API.SetNuiFocus(false, false);
            API.ShutdownLoadingScreen();
        }

        public async Task Load(CuriosityPlayer player)
        {
            API.SetClockTime(12, 1, 0);
            API.SetWeatherTypeNow("EXTRASUNNY");

            EventSystem.Send("user:queue:active");

            API.ShutdownLoadingScreen();
            API.ShutdownLoadingScreenNui();

            Screen.Fading.FadeOut(0);

            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(100);
            }

            Instance.DiscordRichPresence.Status = $"Loading...";
            Instance.DiscordRichPresence.Commit();

            var transition = new LoadTransition();

            if (!player.Character.MarkedAsRegistered)
            {
                API.StopPlayerSwitch();
            }

            var character = player.Character;
            var position = !character.MarkedAsRegistered
                ? CharacterExtensions.RegistrationPosition
                : character.LastPosition ?? CharacterExtensions.DefaultPosition;

            if (position == character.LastPosition) position.Y += 1f;

            character.Revive(position);

            var ped = Cache.Entity.Id;
            var health = character.Health;

            if (health > API.GetEntityMaxHealth(ped))
                health = API.GetEntityMaxHealth(ped);

            if (character.IsDead)
            {
                Cache.PlayerPed.Health = 0;
                Cache.PlayerPed.Armor = 0;
                Cache.PlayerPed.Kill();
            }
            else
            {
                Cache.PlayerPed.Health = health;
                Cache.PlayerPed.Armor = character.Armor;
            }

            Logger.Info("[Character] Base Settings Loaded...");

            // INVENTORIES

            // Load
            Logger.Info("[Character] Inventories Loaded...");

            await player.Character.Load();

            Logger.Info("[Character] Joining Session...");

            await SafeTeleport.Teleport(API.PlayerPedId(), position);

            Logger.Info("[Character] Teleported...");

            Logger.Debug($"Character Registered: {player.Character.MarkedAsRegistered}");

            if (player.Character.MarkedAsRegistered)
            {
                await transition.Wait();
                Screen.Fading.FadeIn(5000);
                await transition.Down(player);
            }
            else
            {
                await player.Character.PostLoad();
            }

            Logger.Info("[Character] Complete Loading...");

            if (Screen.Fading.IsFadedOut && !Screen.Fading.IsFadingOut)
            {
                Screen.Fading.FadeIn(5000);
            }
            Instance.DiscordRichPresence.Status = $"Roaming around...";
            Instance.DiscordRichPresence.Commit();

            Screen.LoadingPrompt.Hide();
            player.EnableHud();

            EventSystem.Send("character:routing:base");
            Instance.ExportDictionary["pma-voice"].toggleMute();

            Cache.UpdatePedId(true);
        }
    }
}
