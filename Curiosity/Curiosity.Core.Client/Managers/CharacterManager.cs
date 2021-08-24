using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
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

            Cache.Player.Character = curiosityCharacter;

            Screen.LoadingPrompt.Show("Loading Character...");

            await Load(Cache.Player);

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

            if (Game.PlayerPed.Exists())
            {
                Game.PlayerPed.Position = new Vector3(405.9228f, -954.1149f, -99.6627f);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.IsInvincible = true;
            }

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

            if (character.IsDead && player.Character.MarkedAsRegistered)
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

            if (!player.Character.MarkedAsRegistered)
            {
                Cache.Character.IsPassive = true;
            }

            PlayerOptionsManager.GetModule().SetPlayerPassiveOnStart(Cache.Character.IsPassive);

            Logger.Debug($"Character Passive State: {Cache.Character.IsPassive}");

            if (player.Character.MarkedAsRegistered)
            {
                Vector3 charPos = position.AsVector();
                Vector3 spawnPos = Vector3.Zero;
                float spawnHeading = 0f;

                Vector3 spawnRoad = Vector3.Zero;

                API.GetClosestVehicleNodeWithHeading(charPos.X, charPos.Y, charPos.Z, ref spawnPos, ref spawnHeading, 1, 3f, 0);
                API.GetRoadSidePointWithHeading(spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, ref spawnRoad);

                spawnRoad.Z = World.GetGroundHeight(spawnRoad) + 2;

                position.X = spawnRoad.X;
                position.Y = spawnRoad.Y;
                position.Z = spawnRoad.Z;
                position.H = spawnHeading;

                Vector3 safePosition = Vector3.Zero;

                if (API.GetSafeCoordForPed(spawnRoad.X, spawnRoad.Y, spawnRoad.Z, true, ref safePosition, 16))
                {
                    position.X = safePosition.X;
                    position.Y = safePosition.Y;
                    position.Z = safePosition.Z;
                }

                Game.PlayerPed.Position = position.AsVector();
                Game.PlayerPed.Heading = position.H;

                await BaseScript.Delay(1000);

                string msg = "You have been moved to the City Hall as you were found";

                if (API.IsEntityInWater(API.PlayerPedId()))
                {
                    Notify.Info($"{msg} sleeping with the fishes.");
                    position = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);
                    Game.PlayerPed.Position = position.AsVector();
                    Game.PlayerPed.Heading = position.H;
                }

                if (API.IsEntityInAir(API.PlayerPedId()))
                {
                    Notify.Info($"{msg} being abducted.");
                    position = new Position(-542.1675f, -216.1688f, -216.1688f, 276.3713f);
                    Game.PlayerPed.Position = position.AsVector();
                    Game.PlayerPed.Heading = position.H;
                }

                await transition.Wait();
                Screen.Fading.FadeIn(5000);
                await transition.Down(player);

                Game.PlayerPed.IsPositionFrozen = false;
                Game.PlayerPed.IsInvincible = false;
            }
            else
            {
                Cache.Character.IsPassive = true;
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

            Cache.PlayerPed.RelationshipGroup = Instance.PlayerRelationshipGroup;

            Cache.UpdatePedId(true);
        }
    }
}
