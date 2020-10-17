using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.DataClasses.Mission;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Static;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Classes.Environment;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace Curiosity.Missions.Client.Scripts.Mission
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

        static public async Task Create(MissionData store)
        {
            if (store == null)
            {
                Debug.WriteLine("[Mission] Create called but store not supplied");
                return;
            }

            // SETUP/RESET
            HostageReleased = false;
            HostageKilled = false;
            Static.Relationships.SetupRelationShips();

            try
            {

                HostageReleased = false;

                SetupLocationBlip(store.Location);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 2", $"{store.Name}", "459S Burglar alarm, silent", 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
                SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} UNITS_RESPOND/UNITS_RESPOND_CODE_02_0{Client.Random.Next(1, 3)} WE_HAVE/WE_HAVE_0{Client.Random.Next(1, 3)} CRIMES/CRIME_ROBBERY_0{Client.Random.Next(1, 5)} RESIDENT/OUTRO_0{Client.Random.Next(1, 4)}");

                client.RegisterTickHandler(MissionCancelAsync);

                while (Game.PlayerPed.Position.Distance(store.Location) > 100f)
                {
                    await BaseScript.Delay(10);
                }

                client.DeregisterTickHandler(MissionCancelAsync);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Log.Info($"SETUP: {store.Name}");
                }

                MissionPedData1 = store.MissionGangOne[0];
                MissionPedData2 = store.MissionGangOne[1];
                MissionPedData3 = store.MissionGangOne[2];
                MissionPedData4 = store.MissionGangOne[3];

                if (store.Hostages != null)
                {
                    if (store.Hostages.Count > 0)
                    {
                        MissionHostage = store.Hostages[0];

                        Vector3 spawnpoint = MissionHostage.SpawnPoint;
                        spawnpoint.Z = spawnpoint.Z - 1f;
                        HostagePed = await PedCreators.PedCreator.CreatePedAtLocation(MissionHostage.Model, spawnpoint, MissionHostage.SpawnHeading);
                        SetBlockingOfNonTemporaryEvents(HostagePed.Handle, true);

                        Decorators.Set(HostagePed.Handle, Client.DECOR_PED_MISSION, true);
                        Decorators.Set(HostagePed.Handle, Client.DECOR_PED_HOSTAGE, true);

                        new AnimationQueue(HostagePed.Handle).PlayDirectInQueue(new AnimationBuilder().Select("random@arrests", "kneeling_arrest_idle").WithFlags(AnimationFlags.Loop));

                        HostagePed.IsPositionFrozen = true;
                    }
                }

                await BaseScript.Delay(10);
                Vector3 mpd1Spawnpoint = MissionPedData1.SpawnPoint;
                Ped ped1 = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData1.Model, mpd1Spawnpoint, MissionPedData1.SpawnHeading);

                if (ped1 != null)
                {
                    ped1.Weapons.Give(MissionPedData1.Weapon, 1, true, true);
                    await BaseScript.Delay(0);
                    MissionPed1 = PedCreators.MissionPedCreator.Ped(ped1, Relationships.HostileRelationship, MissionPedData1.Alertness, MissionPedData1.Difficulty, MissionPedData1.VisionDistance);
                }

                await BaseScript.Delay(10);

                Vector3 mpd2Spawnpoint = MissionPedData2.SpawnPoint;
                Ped ped2 = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData2.Model, mpd2Spawnpoint, MissionPedData2.SpawnHeading);

                if (ped2 != null)
                {
                    ped2.Weapons.Give(MissionPedData2.Weapon, 1, true, true);
                    await BaseScript.Delay(0);
                    MissionPed2 = PedCreators.MissionPedCreator.Ped(ped2, Relationships.HostileRelationship, MissionPedData2.Alertness, MissionPedData2.Difficulty, MissionPedData2.VisionDistance);
                }

                await BaseScript.Delay(10);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                {
                    Log.Info($"INITAL MISSION PEDS: {store.Name}");

                    if (MissionPed1 != null)
                        Log.Info($"PED1: {MissionPed1.Exists()}");
                    if (MissionPed2 != null)
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
                    Vector3 spawnpoint = MissionPedData3.SpawnPoint;
                    Ped backup = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData3.Model, spawnpoint, MissionPedData3.SpawnHeading);

                    if (backup != null)
                    {
                        backup.Weapons.Give(MissionPedData3.Weapon, 1, true, true);
                        await Client.Delay(0);
                        MissionPed3 = PedCreators.MissionPedCreator.Ped(backup, Relationships.HostileRelationship, MissionPedData3.Alertness, MissionPedData3.Difficulty, MissionPedData3.VisionDistance);
                    }

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
                    Vector3 spawnpoint = MissionPedData4.SpawnPoint;
                    Ped backup = await PedCreators.PedCreator.CreatePedAtLocation(MissionPedData4.Model, spawnpoint, MissionPedData4.SpawnHeading);

                    if (backup != null)
                    {
                        backup.Weapons.Give(MissionPedData4.Weapon, 1, true, true);
                        await Client.Delay(0);
                        MissionPed4 = PedCreators.MissionPedCreator.Ped(backup, Relationships.HostileRelationship, MissionPedData4.Alertness, MissionPedData4.Difficulty, MissionPedData4.VisionDistance);
                    }

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

                                if (HostageReleased)
                                {
                                    HostagePed.IsPositionFrozen = false;
                                }

                                await BaseScript.Delay(0);
                            }
                        }
                    }

                    if (HostageReleased && Game.PlayerPed.IsAlive)
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

            if (HostageReleased && Game.PlayerPed.IsAlive)
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
                client.DeregisterTickHandler(MissionCancelAsync);
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

                if (!RandomMissionHandler.IsOnDuty)
                {
                    Screen.DisplayHelpTextThisFrame($"Mission Cancelled");
                    break;
                }
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

            RandomMissionHandler.SetDispatchMessageRecieved(false);

            SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Client.Random.Next(1, 5)} RESIDENT/OUTRO_0{Client.Random.Next(1, 4)}");

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
