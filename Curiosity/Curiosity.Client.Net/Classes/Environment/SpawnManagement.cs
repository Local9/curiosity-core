using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Client.net.Classes.Player;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    class SpawnManagement
    {
        static List<Vector3> hospitals = new List<Vector3>()
        {
            new Vector3(297.8683f, -584.3318f, 43.25863f),
            new Vector3(356.434f, -598.5284f, 28.78098f),
            new Vector3(307.5486f, -1434.502f, 29.86082f),
            new Vector3(342.1533f, -1397.199f, 32.50924f),
            new Vector3(-496.291f, -336.9345f, 34.50164f),
            new Vector3(-449.0542f, -339.1804f, 34.50176f),
            new Vector3(1827.909f, 3691.912f, 34.22427f),
            new Vector3(-243.5568f, 6326.441f, 32.42619f),
            new Vector3(4773.674f, -1935.709f, 17.06867f)
        };

        static Random rnd = new Random();
        static Client client = Client.GetInstance();
        static bool hasPlayerSpawned = false;
        static Vehicle CurrentVehicle = null;

        static bool deathReset = false;

        static int seconds = 300;
        static long gameTime = API.GetGameTimer();

        static private string _currentJob = "Unknown";

        static bool CooldownActive = false;

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleIdUpdate));
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDuty));

            client.RegisterEventHandler("curiosity:Client:Player:Revive", new Action(OnRevive));

            client.RegisterTickHandler(OnWastedCheck);
            client.RegisterTickHandler(OnRevivePlayerCheck);

            foreach (Vector3 pos in hospitals)
            {
                API.AddHospitalRestart(pos.X, pos.Y, pos.Z - 1.0f, 0.0f, 0);
            }
        }

        private static async Task OnReviveCooldown()
        {
            long GameTimer = API.GetGameTimer();

            while ((API.GetGameTimer() - GameTimer) < 30000)
            {
                await Client.Delay(1000);
            }

            CooldownActive = false;
            Screen.ShowNotification($"~g~Revive Cooldown Cleared");
            client.DeregisterTickHandler(OnReviveCooldown);
        }

        private static async Task OnRevivePlayerCheck()
        {
            try
            {
                CitizenFX.Core.Player closestPlayer = Client.players.Select(x => x).Where(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position) < 2f && p.Character.IsDead).FirstOrDefault();

                if (closestPlayer == null) return;

                //if (PlayerInformation.IsDeveloper())
                //{
                //    Log.Info($"Player to revive: {closestPlayer.Name}");
                //    Log.Info($"----- > Revive Checks < -----");
                //    Log.Info($"isSessionActive: {Client.isSessionActive}");
                //    Log.Info($"IsInVehicle: {Game.PlayerPed.IsInVehicle()}");
                //    Log.Info($"notSamePlayer: {closestPlayer != Game.Player}");
                //    Log.Info($"Player IsAlive: {Game.Player.IsAlive}");
                //    Log.Info($"----- > Revive Checks < -----");
                //}

                if (
                    Client.isSessionActive
                    && closestPlayer != null
                    && !Game.PlayerPed.IsInVehicle()
                    && closestPlayer != Game.Player
                    && Game.PlayerPed.IsAlive
                    )
                {
                    Screen.DisplayHelpTextThisFrame($"~w~Press ~INPUT_CONTEXT~ to revive ~b~{closestPlayer.Name}~w~...");

                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        if (CooldownActive)
                        {
                            Screen.ShowNotification($"~r~Cooldown Active");
                        }
                        else
                        {
                            Debug.WriteLine($"Reviving Player {closestPlayer.Name}|{closestPlayer.ServerId}");

                            Client.TriggerServerEvent("curiosity:Server:Player:Revive", closestPlayer.ServerId, false);
                            CooldownActive = true;
                            client.RegisterTickHandler(OnReviveCooldown);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Revive > Critical Error: {ex.Message}");
            }
        }

        public static async void OnRevive()
        {
            Debug.WriteLine($"Reviving");

            API.NetworkFadeOutEntity(Game.PlayerPed.Handle, false, false);
            Game.PlayerPed.IsPositionFrozen = true;
            await Client.Delay(500);

            Vector3 pos = Game.PlayerPed.Position;
            API.NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z, 0.0f, false, false);

            Game.PlayerPed.Health = 50;
            Game.PlayerPed.Armor = 0;

            hasPlayerSpawned = true;

            API.SetFakeWantedLevel(0);

            API.SetResourceKvpInt("DEATH", 0);
            deathReset = true;
            Screen.Effects.Stop(ScreenEffect.DeathFailOut);

            await Client.Delay(500);
            API.NetworkFadeInEntity(Game.PlayerPed.Handle, false);
            Game.PlayerPed.IsPositionFrozen = false;
        }

        static void OnDuty(bool active, bool onDuty, string job)
        {
            _currentJob = job;
        }

        static void OnVehicleIdUpdate(int vehicleId)
        {
            if (API.IsAnEntity(vehicleId))
                CurrentVehicle = new Vehicle(vehicleId);
        }

        static void OnPlayerSpawned()
        {
            hasPlayerSpawned = true;
            deathReset = true;
        }

        static async Task ShowText()
        {
            long time = seconds - (int)((API.GetGameTimer() - gameTime) / 1000);
            string messageText = $"~w~Wait {time}s or press ~b~E~w~ to force respawn.~n~~r~NOTE: You will be charged if you force respawn";
            string messageText2 = $"~n~Other players can revive you, but it will cost ~g~$50~w~";

            NativeWrappers.Draw3DText(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z - 0.4f, messageText, 75f, 10f);
            NativeWrappers.Draw3DText(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z - 0.5f, messageText2, 75f, 10f);

            if (Player.PlayerInformation.IsDeveloper())
            {
                Debug.WriteLine(messageText);
            }

            if (Game.PlayerPed.IsAlive)
                client.DeregisterTickHandler(ShowText);
        }

        static async Task OnWastedCheck()
        {
            try
            {
                if (!hasPlayerSpawned) return;

                if (Game.PlayerPed.IsAlive && hasPlayerSpawned && !deathReset)
                {
                    int deathCheck = API.GetResourceKvpInt("DEATH");

                    if (deathCheck > 0)
                    {
                        API.SetResourceKvpInt("DEATH", 0);
                        deathReset = true;
                    }
                }

                if (Game.PlayerPed.IsDead && hasPlayerSpawned)
                {
                    API.SetResourceKvpInt("DEATH", 1);

                    Screen.Effects.Start(ScreenEffect.DeathFailOut, 0);

                    UI.Scaleforms.Wasted();

                    await Client.Delay(1000);

                    bool forceRespawn = false;

                    gameTime = API.GetGameTimer();

                    client.RegisterTickHandler(ShowText);

                    if (!Player.PlayerInformation.IsStaff())
                    {
                        while (!forceRespawn)
                        {
                            await Client.Delay(0);

                            if (Game.IsControlJustPressed(0, Control.Pickup))
                            {
                                forceRespawn = true;
                            }

                            if (Game.PlayerPed.IsAlive)
                                break;

                            if ((API.GetGameTimer() - gameTime) > (1000 * seconds))
                            {
                                break;
                            }
                        }
                    }

                    if (!Game.PlayerPed.IsAlive)
                    {
                        client.DeregisterTickHandler(ShowText);

                        API.SetResourceKvpInt("DEATH", 0);

                        await Client.Delay(2000);

                        API.DoScreenFadeOut(1000);

                        while (!API.IsScreenFadedOut())
                        {
                            await Client.Delay(0);
                        }

                        Screen.Effects.Stop(ScreenEffect.DeathFailOut);

                        int r = rnd.Next(hospitals.Count);

                        Vector3 playerPos = Game.PlayerPed.Position;

                        Vector3 pos = new Vector3();

                        foreach (Vector3 hosPos in hospitals)
                        {
                            float distance = API.GetDistanceBetweenCoords(playerPos.X, playerPos.Y, playerPos.Z, hosPos.X, hosPos.Y, hosPos.Z, false);

                            if (distance < 3000f)
                            {
                                pos = hosPos;
                                break;
                            }
                        }

                        if (pos.IsZero)
                        {
                            pos = hospitals[r];
                            UI.Notifications.LifeV(1, "EMS", "", "Looks like you had a bad coma...", 132);
                        }

                        Game.PlayerPed.Position = new Vector3(pos.X, pos.Y, pos.Z - 1.0f);

                        Game.PlayerPed.IsPositionFrozen = true;
                        API.NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z - 1.0f, 0.0f, false, false);

                        await Client.Delay(100);
                        Game.PlayerPed.IsPositionFrozen = false;
                        Game.PlayerPed.DropsWeaponsOnDeath = false;
                        // Game.PlayerPed.Weapons.RemoveAll();

                        Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, _currentJob);

                        await Client.Delay(100);

                        if (CurrentVehicle != null)
                        {
                            if (CurrentVehicle.Exists())
                            {
                                SendDeletionEvent(CurrentVehicle.NetworkId);
                                CurrentVehicle.Delete();
                            }
                        }

                        API.DoScreenFadeIn(1000);

                        while (!API.IsScreenFadedIn())
                        {
                            await Client.Delay(0);
                        }

                        API.SetFakeWantedLevel(0);
                        Client.TriggerServerEvent("curiosity:Server:Bank:MedicalFees", forceRespawn);

                        if (forceRespawn)
                        {
                            UI.Notifications.LifeV(1, "EMS", "Medical Fees", "You have been charged for your stay, please try to stay alive.", 132);
                        }
                        else
                        {
                            UI.Notifications.LifeV(1, "EMS", "Medical Fees", "You've been treated for you injuries.", 132);
                        }
                    }
                }
                else
                {
                    if (hasPlayerSpawned && !deathReset)
                    {
                        API.SetResourceKvpInt("DEATH", 0);
                        deathReset = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnWastedCheck -> {ex.Message}");
            }
        }
        static void SendDeletionEvent(int vehicleNetworkId)
        {
            string encodedString = Encode.StringToBase64($"{vehicleNetworkId}");
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }
    }

}

