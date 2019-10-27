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
using Curiosity.Shared.Client.net.Enums.Patrol;


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
        static Position Location = new Position();

        static public async void Create(Store store)
        {
            if (store == null)
            {
                Debug.WriteLine("[Mission] Create called but store not supplied");
                return;
            }

            try
            {

                RandomMissionHandler.SetIsOnActiveCallout(true);

                SetupLocationBlip(store.Location);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "459S Burglar alarm, silent", $"{store.Name}", string.Empty, 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                client.RegisterTickHandler(MissionCancelAsync);

                while (Game.PlayerPed.Position.DistanceToSquared(store.Location) > 1000)
                {
                    await BaseScript.Delay(0);
                }


                MissionPedData1 = store.missionPeds[0];
                MissionPedData2 = store.missionPeds[1];
                MissionPedData3 = store.missionPeds[2];
                MissionPedData4 = store.missionPeds[3];

                if (store.hostages.Count > 0)
                {
                    MissionHostage = store.hostages[0];
                    HostagePed = await PedCreators.PedCreator.CreatePedAtLocation(MissionHostage.Model, MissionHostage.SpawnPoint, MissionHostage.SpawnHeading);
                    SetBlockingOfNonTemporaryEvents(HostagePed.Handle, true);
                    new AnimationQueue(HostagePed.Handle).PlayDirectInQueue(new AnimationBuilder().Select("random@arrests", "kneeling_arrest_idle").WithFlags(AnimationFlags.Loop));
                }

                MissionPed1 = await CreatePed(MissionPedData1);
                MissionPed2 = await CreatePed(MissionPedData2);

                client.RegisterTickHandler(SpawnBackupPedOne);
                client.RegisterTickHandler(SpawnBackupPedTwo);

                client.RegisterTickHandler(MissionCompletionChecks);
            }
            catch (Exception ex)
            {
                Log.Error("[CreateStoreMission] Mission filed creation");
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

                    MissionPed3 = PedCreators.MissionPedCreator.Ped(backup, MissionPedData3.Alertness, MissionPedData3.Difficulty, MissionPedData3.VisionDistance);

                    if (backup != null)
                    {
                        client.DeregisterTickHandler(SpawnBackupPedOne);
                        running = false;
                    }
                }
                await BaseScript.Delay(500);
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

                    MissionPed4 = PedCreators.MissionPedCreator.Ped(backup, MissionPedData4.Alertness, MissionPedData4.Difficulty, MissionPedData4.VisionDistance);

                    if (backup != null)
                    {
                        client.DeregisterTickHandler(SpawnBackupPedTwo);
                        running = false;
                    }
                }
                await BaseScript.Delay(500);
            }
        }

        static async Task MissionCompletionChecks()
        {
            await Task.FromResult(0);

            if (MissionHostage != null)
            {
                if (HostagePed.IsAlive)
                {
                    if (HostagePed.Position.DistanceToSquared(Game.PlayerPed.Position) < 2f && !HostageReleased)
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

            RemoveEntity(MissionPed1);
            RemoveEntity(MissionPed2);
            RemoveEntity(MissionPed3);
            RemoveEntity(MissionPed4);

            if (cancelMission)
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Callout Cancelled", $"No Payout", string.Empty, 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
            }
            else
            {
                Client.TriggerServerEvent("curiosity:Server:Missions:CompletedMission", !HostageKilled);
            }

            Position position = new Position(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);

            while (Location.Distance(position) < 100f)
            {
                await BaseScript.Delay(0);
                Screen.DisplayHelpTextThisFrame($"Please leave the area");
                position = new Position(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
            }

            HostageReleased = false;
            HostageKilled = false;

            MissionPedData1 = null;
            MissionPedData2 = null;
            MissionPedData3 = null;
            MissionPedData4 = null;
            MissionHostage = null;

            RemoveEntity(HostagePed);

            ClearAreaOfPeds(LocationBlip.Position.X, LocationBlip.Position.Y, LocationBlip.Position.Z, 200f, 1);

            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                {
                    LocationBlip.Delete();
                }
            }

            RandomMissionHandler.SetIsOnActiveCallout(false);

            if (RandomMissionHandler.IsOnDuty)
                RandomMissionHandler.AllowNextMission();
        }

        static async Task<MissionPed> CreatePed(MissionPedData missionPedData)
        {
            Ped backup = await PedCreators.PedCreator.CreatePedAtLocation(missionPedData.Model, missionPedData.SpawnPoint, missionPedData.SpawnHeading);
            backup.Weapons.Give(missionPedData.Weapon, 1, true, true);
            return PedCreators.MissionPedCreator.Ped(backup, missionPedData.Alertness, missionPedData.Difficulty, missionPedData.VisionDistance);
        }

        static bool AreMissionPedsDead()
        {
            return (!NativeWrappers.IsEntityAlive(MissionPed1) || NativeWrappers.IsEntityAlive(MissionPed2));
        }

        static void SetupLocationBlip(Vector3 location)
        {
            Location.X = location.X;
            Location.Y = location.Y;
            Location.Z = location.Z;

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
