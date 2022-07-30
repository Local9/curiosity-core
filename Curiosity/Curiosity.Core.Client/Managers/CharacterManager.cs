using CitizenFX.Core.UI;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.Milo;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System.Text;

namespace Curiosity.Core.Client.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public CuriosityCharacter curiosityCharacter = new CuriosityCharacter();
        Vector3 CityHallPosition = new Vector3(-542.1675f, -216.1688f, -216.1688f);
        public override void Begin()
        {
            EventSystem.Attach("character:model", new AsyncEventCallback(async metadata =>
            {

                string modelStr = metadata.Find<string>(0);

                if (string.IsNullOrEmpty(modelStr)) return null;

                Model model = modelStr;
                await model.Request(1000);

                if (!model.IsLoaded) return null;
                await Game.PlayerPed.FadeOut();
                Game.Player.ChangeModel(model);
                Game.PlayerPed.FadeIn();

                return null;
            }));
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
            EventSystem.Send("user:job", "Loading...");

            API.ShutdownLoadingScreen();
            API.ShutdownLoadingScreenNui();

            if (Game.PlayerPed.Exists())
            {
                // Game.PlayerPed.Position = new Vector3(405.9228f, -954.1149f, -99.6627f);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.IsInvincible = true;
                Game.PlayerPed.IsCollisionEnabled = false;
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
                player.Character.IsPassive = true;
            }

            var character = player.Character;
            var position = !character.MarkedAsRegistered
                ? CharacterExtensions.RegistrationPosition
                : character.LastPosition ?? CharacterExtensions.DefaultPosition;

            Logger.Debug($"[SPAWN POSITION] {position}");
            character.Revive(position);

            var ped = Cache.Entity.Id;
            var health = character.Health;

            if (health > API.GetEntityMaxHealth(ped))
                health = API.GetEntityMaxHealth(ped);

            Cache.PlayerPed.Health = health;
            Cache.PlayerPed.Armor = character.Armor;

            Cache.PlayerPed.FadeOut();

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
                await BaseScript.Delay(1000);

                CayoPericoManager cayoPericoManager = CayoPericoManager.GetModule();

                Game.PlayerPed.IsCollisionEnabled = true;
                Game.PlayerPed.IsPositionFrozen = false;

                if (Cache.Character.IsWanted)
                {
                    EventSystem.Send("police:suspect:jail:combatLogged");
                    Cache.Character.IsOnIsland = false;

                    Notify.Warn($"You were found by the LSPD and jailed, the courts have taken 5% of your money, and sentenced you to 6 minutes in jail, failure to comply will lead to further fines.");
                }
                else if (Cache.Character.IsOnIsland) // This is done to get around an IPL Bug in GTA
                {
                    Cache.PlayerPed.Position = new Vector3(-1016.42f, -2468.58f, 12.99f);
                    Cache.PlayerPed.Heading = 233.31f;
                    Cache.Character.IsOnIsland = false;
                }
                else if (!Cache.Character.IsOnIsland)
                {
                    cayoPericoManager.SetupLosSantos();

                    Vector3 pos = Game.PlayerPed.Position;
                    float groundZ = pos.Z;
                    if (API.GetGroundZFor_3dCoord(pos.X, pos.Y, pos.Z, ref groundZ, false))
                        Game.PlayerPed.Position = new Vector3(pos.X, pos.Y, groundZ);

                    Vector3 currentPos = Cache.PlayerPed.Position = Game.PlayerPed.Position;
                    int interiorId = GetInteriorFromEntity(Game.PlayerPed.Handle);

                    Logger.Debug($"Interior {interiorId} @ {currentPos.X},{currentPos.Y},{currentPos.Z}");

                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.Append("You have been moved to the City Hall as you were found");
                    bool sendMessage = false;

                    if (interiorId > 0)
                    {
                        sendMessage = true;
                        strBuilder.Append(" in some weird location.");
                        MoveToCityHall();
                    }

                    float waterHeight = pos.Z;
                    currentPos = Cache.PlayerPed.Position = Game.PlayerPed.Position;

                    if (API.TestVerticalProbeAgainstAllWater(currentPos.X, currentPos.Y, currentPos.Z, 1, ref waterHeight))
                    {
                        currentPos.Z = waterHeight;
                    }

                    Game.PlayerPed.Position = currentPos;

                    if (API.IsEntityInWater(API.PlayerPedId()))
                    {
                        sendMessage = true;
                        strBuilder.Append(" sleeping with the fishes.");
                        MoveToCityHall();
                    }

                    if (API.IsEntityInAir(API.PlayerPedId()))
                    {
                        sendMessage = true;
                        strBuilder.Append(" being abducted.");
                        MoveToCityHall();
                    }

                    if (sendMessage)
                    {
                        NotificationManager.GetModule().Info(strBuilder.ToString());
                    }

                    if (Game.PlayerPed.IsInRangeOf(CityHallPosition, 300f))
                        MoveToCityHall();

                    await BaseScript.Delay(100);

                    Vector3 curPosition = Cache.Character.LastPosition.AsVector();
                    Vector3 newPosition = curPosition;
                    API.GetSafeCoordForPed(curPosition.X, curPosition.Y, curPosition.Z, true, ref newPosition, 0);

                    await BaseScript.Delay(100);

                    Game.PlayerPed.Position = newPosition;
                }

                WorldManager.GetModule().UpdateWeather(true);

                await transition.Wait();
                Screen.Fading.FadeIn(2500);
                await transition.Down(player);
                Game.PlayerPed.IsInvincible = false;
            }
            else
            {
                EventSystem.Send("user:job", "Character Creation");
                Cache.Character.IsPassive = true;
                await player.Character.PostLoad();
            }

            Logger.Info("[Character] Complete Loading...");

            TriggerMusicEvent($"{Instance.ClientMusicEvent.Stop}");

            PopulateNow();
            if (Screen.Fading.IsFadedOut && !Screen.Fading.IsFadingOut)
            {
                Screen.Fading.FadeIn(2500);
            }

            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(0);
            }

            CancelMusicEvent($"{Instance.ClientMusicEvent.Start}");
            CancelMusicEvent($"{Instance.ClientMusicEvent.Stop}");

            Common.LoadMissingMapObjects();

            Instance.DiscordRichPresence.Status = $"Roaming around...";
            Instance.DiscordRichPresence.Commit();

            Screen.LoadingPrompt.Hide();
            player.EnableHud();

            EventSystem.Send("character:routing:base");

            PlayerOptionsManager.GetModule().SetPlayerPassiveOnStart(Cache.Character.IsPassive);
            Logger.Debug($"Character Passive State: {Cache.Character.IsPassive}");
            SetPedHelmet(Cache.PlayerPed.Handle, Cache.Character.AllowHelmet);

            Game.PlayerPed.RelationshipGroup = Instance.PlayerRelationshipGroup;
            Game.PlayerPed.IsInvincible = false;

            EventSystem.Send("user:job", "Unemployed");

            // CreatePlayerGroup();
            SetPedMinGroundTimeForStungun(Game.PlayerPed.Handle, 10000);

            Vector3 p = Game.PlayerPed.Position;
            var spawn = new { x = p.X, y = p.Y, z = p.Z, heading = Game.PlayerPed.Heading, model = Game.PlayerPed.Model.Hash };
            BaseScript.TriggerEvent("playerSpawned", spawn);

            Notify.CanSendNotification = true;

            TriggerMusicEvent($"{MusicEvents.DEFAULT_STOP}");
            CancelMusicEvent($"{MusicEvents.DEFAULT_STOP}");
        }

        private static void CreatePlayerGroup()
        {
            PedGroup pedGroup;

            if (Cache.PlayerPed.IsInGroup)
            {
                pedGroup = Cache.PlayerPed.PedGroup;
                Logger.Debug($"Player already in a group: Player Group: {Cache.PlayerPed.PedGroup.Handle}");
                goto SetupGroup;
            }

            pedGroup = new PedGroup();

        SetupGroup:
            pedGroup.Add(Cache.PlayerPed, true);
            Cache.PedGroup = pedGroup;

            pedGroup.FormationType = FormationType.Default;
            pedGroup.SeparationRange = 300f;
            SetGroupFormationSpacing(pedGroup.Handle, 1f, 0.9f, 3f);

            if (Cache.PlayerPed.PedGroup is not null)
                Logger.Debug($"Player Group: {Cache.PlayerPed.PedGroup.Handle}");
        }

        private void MoveToCityHall()
        {
            Logger.Debug($"Moving to City Hall");
            Game.PlayerPed.Position = CityHallPosition;
            Game.PlayerPed.Heading = 276.3713f;
        }
    }
}
