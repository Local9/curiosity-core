using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses.Mission;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Classes.Data;
using Curiosity.Shared.Client.net.Classes.Environment;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class CreateStoreMission
    {
        static Client client = Client.GetInstance();

        static MissionPedData MissionPedData1;
        static MissionPedData MissionPedData2;
        static MissionPedData MissionPedData3;
        static MissionPedData MissionPedData4;

        static MissionPedData MissionHostage;

        static MissionPed MissionPed1;
        static MissionPed MissionPed2;
        static MissionPed MissionPed3;
        static MissionPed MissionPed4;

        static Ped HostagePed;
        static bool HostageReleased = false;
        static bool HostageKilled = false;

        static Blip LocationBlip;
        static Vector3 Location = new Vector3();
        
        static public async Task Create(Store store)
        {
            if (store == null)
            {
                Debug.WriteLine("[Mission] Create called but store not supplied");
                return;
            }

            Static.Relationships.SetupRelationShips();

            try
            {

                SetupLocationBlip(store.Location);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 2", $"{store.Name}", "459S Burglar alarm, silent", 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                while (Game.PlayerPed.Position.Distance(store.Location) > 50f)
                {
                    await BaseScript.Delay(0);
                }

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Log.Info($"SETUP: {store.Name}");
                }

                MissionPedData1 = store.missionPeds[0];
                MissionPedData2 = store.missionPeds[1];
                MissionPedData3 = store.missionPeds[2];
                MissionPedData4 = store.missionPeds[3];

                if (store.hostages != null)
                {
                    if (store.hostages.Count > 0)
                    {
                        MissionHostage = store.hostages[0];
                        HostagePed = await PedCreators.PedCreator.CreatePedAtLocation(MissionHostage.Model, MissionHostage.SpawnPoint, MissionHostage.SpawnHeading);
                        SetBlockingOfNonTemporaryEvents(HostagePed.Handle, true);
                        new AnimationQueue(HostagePed.Handle).PlayDirectInQueue(new AnimationBuilder().Select("random@arrests", "kneeling_arrest_idle").WithFlags(AnimationFlags.Loop));
                        SetPedScream(HostagePed.Handle);
                    }
                }

                await BaseScript.Delay(10);

                Ped ped1 = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData1.Model, MissionPedData1.SpawnPoint, MissionPedData1.SpawnHeading);
                ped1.Weapons.Give(MissionPedData1.Weapon, 1, true, true);
                await Client.Delay(0);
                MissionPed1 = PedCreators.MissionPedCreator.Ped(ped1, MissionPedData1.Alertness, MissionPedData1.Difficulty, MissionPedData1.VisionDistance);
                
                await BaseScript.Delay(0);
                
                Ped ped2 = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData2.Model, MissionPedData2.SpawnPoint, MissionPedData2.SpawnHeading);
                ped2.Weapons.Give(MissionPedData2.Weapon, 1, true, true);
                await Client.Delay(0);
                MissionPed2 = PedCreators.MissionPedCreator.Ped(ped2, MissionPedData2.Alertness, MissionPedData2.Difficulty, MissionPedData2.VisionDistance);
                
                await BaseScript.Delay(500);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Log.Info($"INITAL MISSION PEDS: {store.Name}");
                    Log.Info($"PED1: {MissionPed1.Exists()}");
                    Log.Info($"PED2: {MissionPed2.Exists()}");
                    Log.Info($"---------------------------------");
                }

                client.RegisterTickHandler(SpawnBackupPedOne);
                client.RegisterTickHandler(SpawnBackupPedTwo);

                client.RegisterTickHandler(MissionCompletionChecks);
            }
            catch (Exception ex)
            {
                Log.Error("[CreateStoreMission] Mission failed creation");

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Log.Error($"{ex}");
                }
            }
        }

        static async Task SpawnBackupPedOne()
        {
            await Task.FromResult(0);
            bool running = true;
            while (running)
            {
                if (AreMissionPedsDead() && Client.Random.Next(3) == 1)
                {
                    Ped backup = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData3.Model, MissionPedData3.SpawnPoint, MissionPedData3.SpawnHeading);
                    backup.Weapons.Give(MissionPedData3.Weapon, 1, true, true);
                    await Client.Delay(0);
                    MissionPed3 = PedCreators.MissionPedCreator.Ped(backup, MissionPedData3.Alertness, MissionPedData3.Difficulty, MissionPedData3.VisionDistance);

                    if (backup != null)
                    {
                        client.DeregisterTickHandler(SpawnBackupPedOne);
                        running = false;
                    }
                }
                await BaseScript.Delay(1000);
            }
        }

        static async Task SpawnBackupPedTwo()
        {
            await Task.FromResult(0);
            bool running = true;
            while (running)
            {
                if (AreMissionPedsDead() && Client.Random.Next(5) == 1)
                {
                    Ped backup = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData4.Model, MissionPedData4.SpawnPoint, MissionPedData4.SpawnHeading);
                    backup.Weapons.Give(MissionPedData4.Weapon, 1, true, true);
                    await Client.Delay(0);
                    MissionPed4 = PedCreators.MissionPedCreator.Ped(backup, MissionPedData4.Alertness, MissionPedData4.Difficulty, MissionPedData4.VisionDistance);

                    if (backup != null)
                    {
                        client.DeregisterTickHandler(SpawnBackupPedTwo);
                        running = false;
                    }
                }
                await BaseScript.Delay(1000);
            }
        }

        static async Task MissionCompletionChecks()
        {
            await Task.FromResult(0);

            await Client.Delay(1000);

            if (MissionHostage != null)
            {
                if (HostagePed.IsAlive)
                {
                    if (HostagePed.Position.Distance(Game.PlayerPed.Position) < 2f && !HostageReleased)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press and hold ~INPUT_PICKUP~ to free the hostage.");

                        if (IsControlPressed(0, (int)Control.Pickup))
                        {
                            long gameTime = GetGameTimer();
                            while (IsControlPressed(0, (int)Control.Pickup) && (GetGameTimer() - gameTime) < 6000 && !HostageReleased)
                            {
                                Screen.DisplayHelpTextThisFrame($"Holding ~INPUT_PICKUP~ [{((GetGameTimer() - gameTime) / 1000) - 5}]");

                                HostageReleased = (GetGameTimer() - gameTime) > 5000;

                                await BaseScript.Delay(0);
                            }
                        }
                    }

                    if (HostageReleased)
                    {
                        HostagePed.Task.FleeFrom(HostagePed.Position);
                        Screen.DisplayHelpTextThisFrame($"Hostage has been freed");
                    }
                }
                else
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Callout Failed", $"... I don't know what to say", $"This will not look good on your record.", 2);
                    PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                    HostageKilled = true;

                    CleanUp();
                }
            }

            if (HostageReleased)
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Callout Completed", $"Hostage Rescued", string.Empty, 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                CleanUp();
            }

            if (Game.PlayerPed.IsDead)
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Callout Failed", $"Unlucky...", string.Empty, 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                CleanUp();
            }
        }

        static async Task MissionCancelAsync()
        {
            await Task.FromResult(0);
            if (Game.IsControlPressed(0, Control.FrontendDelete))
            {
                CleanUp(true);
            }
        }

        static void RemoveEntity(Entity ent)
        {
            if (ent == null) return;

            if (ent.Exists())
            {
                NetworkFadeOutEntity(ent.Handle, false, false);
                ent.Delete();
            }
        }

        static public async void CleanUp(bool cancelMission = false)
        {
            client.DeregisterTickHandler(SpawnBackupPedOne);
            client.DeregisterTickHandler(SpawnBackupPedTwo);
            client.DeregisterTickHandler(MissionCompletionChecks);
            client.DeregisterTickHandler(MissionCancelAsync);

            LocationBlip.ShowRoute = false;
            LocationBlip.Scale = 1.0f;

            if (cancelMission)
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Callout Cancelled", $"No Payout", string.Empty, 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
            }
            else
            {
                Client.TriggerServerEvent("curiosity:Server:Missions:CompletedMission", !HostageKilled);
            }

            Vector3 position = Game.PlayerPed.Position;

            while (Location.Distance(position) < 100f)
            {
                await BaseScript.Delay(0);
                Screen.DisplayHelpTextThisFrame($"Please leave the area");
                position = Game.PlayerPed.Position;
            }

            if (!cancelMission)
                Screen.DisplayHelpTextThisFrame($"Thank you");

            RemoveEntity(MissionPed1);
            RemoveEntity(MissionPed2);
            RemoveEntity(MissionPed3);
            RemoveEntity(MissionPed4);
            RemoveEntity(HostagePed);

            HostageReleased = false;
            HostageKilled = false;

            MissionPedData1 = null;
            MissionPedData2 = null;
            MissionPedData3 = null;
            MissionPedData4 = null;
            MissionHostage = null;

            ClearAreaOfPeds(LocationBlip.Position.X, LocationBlip.Position.Y, LocationBlip.Position.Z, 200f, 1);

            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                {
                    LocationBlip.Delete();
                }
            }

            RandomMissionHandler.SetIsOnActiveCallout(false);

            RandomMissionHandler.AllowNextMission();
        }

        static bool AreMissionPedsDead()
        {
            return (!NativeWrappers.IsEntityAlive(MissionPed1) || NativeWrappers.IsEntityAlive(MissionPed2));
        }

        static void SetupLocationBlip(Vector3 location)
        {
            Location = location;

            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                {
                    LocationBlip.Delete();
                }
            }

            LocationBlip = new Blip(AddBlipForCoord(location.X, location.Y, location.Z));
            LocationBlip.Sprite = BlipSprite.BigCircle;
            LocationBlip.Scale = 0.5f;
            LocationBlip.Color = (BlipColor)5;
            LocationBlip.Alpha = 126;
            LocationBlip.ShowRoute = true;
            LocationBlip.Priority = 9;
            LocationBlip.IsShortRange = true;

            SetBlipDisplay(LocationBlip.Handle, 5);
        }
    }
}
