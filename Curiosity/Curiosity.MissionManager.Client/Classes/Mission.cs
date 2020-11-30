using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Utils;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client
{
    public abstract class Mission
    {
        public static EventSystem EventSystem => EventSystem.GetModule();

        internal static List<Type> missions = new List<Type>();
        internal static bool isOnMission = false;
        internal static Mission currentMission = null;
        internal static Type missionType = null;
        public static bool isMessagingServer = false;
        public static bool isEndingMission = false;
        internal static MissionData currentMissionData;
        internal static PluginManager Instance => PluginManager.Instance;
        public static PatrolZone PatrolZone = PatrolZone.Anywhere;

        public static EndState endState = EndState.Unknown;
        static DateTime LastUpdate = DateTime.Now.AddSeconds(5);

        public static List<Player> Players { get; internal set; }
        public static List<Vehicle> RegisteredVehicles { get; internal set; } = new List<Vehicle>();
        public static List<Ped> RegisteredPeds { get; internal set; } = new List<Ped>();
        public static List<ParticleEffect> RegisteredParticles { get; internal set; } = new List<ParticleEffect>();

        public static int NumberPedsArrested { get; internal set; } = 0;
        public static int NumberTransportArrested { get; internal set; } = 0;

        public static void AddPlayer(Player player)
        {
            Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, true);

            if (Players == null)
            {
                Players = new List<Player> { player };
                return;
            }

            Players.Add(player);
        }

        public static void RemovePlayer(Player player)
        {
            Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, false);
            Players.Remove(player);
        }

        /// <summary>
        /// This is called when a mission is started by the user. Typically this would be used to set up the mission by spawning things like peds and vehicles
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// This is called as soon as the mission is ended. Typically this would be used for cleanup such as removing peds and vehicles
        /// </summary>
        public abstract void End();

        public static void CreateParticleAtLocation(string dict, string fx, Vector3 location, float scale = 1.0f, bool placeOnGround = true)
        {
            BaseScript.TriggerServerEvent("s:mm:particle:location", dict, fx, location.X, location.Y, location.Z, scale, placeOnGround);
        }

        public static void CountArrest()
        {
            NumberPedsArrested++;
        }

        public static void ResetCountOfArrested() => NumberPedsArrested = 0;

        public static void RegisterVehicle(Vehicle vehicle)
        {
            try
            {
                vehicle.Fx.IsPersistent = true;
                RegisteredVehicles.Add(vehicle);
                Logger.Debug($"Registered vehicle {vehicle.Hash}");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        public static void RegisterPed(Ped ped)
        {
            try
            {
                ped.Fx.IsPersistent = true;
                RegisteredPeds.Add(ped);
                Logger.Debug($"Registered ped {ped.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        public static void RegisterBlip(Blip blip)
        {
            try
            {
                PluginManager.Blips.Add(blip);
                Logger.Debug($"Registered blip {blip.Handle}");
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        internal static void CountTransportArrest()
        {
            NumberTransportArrested++;
        }

        /// <summary>
        /// Stops a mission
        /// </summary>
        /// <param name="reason">The reason the mission was stopped</param>
        public void Stop(EndState reason)
        {
            if (isMessagingServer) return;
            isMessagingServer = true;

            try
            {
                switch (reason)
                {
                    case EndState.Pass:
                        MissionDirectorManager.GameTimeTillNextMission = DateTime.Now.AddMinutes(Utility.RANDOM.Next(2, 4));
                        break;
                    case EndState.Fail:
                        MissionDirectorManager.GameTimeTillNextMission = DateTime.Now.AddMinutes(Utility.RANDOM.Next(3, 6));
                        break;
                    case EndState.ForceEnd:
                        MissionDirectorManager.GameTimeTillNextMission = DateTime.Now.AddMinutes(Utility.RANDOM.Next(4, 10));
                        Notify.DispatchAI("Callout Ended", $"We'll take note of this Officer {Game.Player.Name}, please complete our calls in the future.");
                        break;
                    case EndState.Error:
                        MissionDirectorManager.GameTimeTillNextMission = DateTime.Now.AddMinutes(1);
                        break;
                }

                isOnMission = false;
                missionType = null;

                currentMission?.End();
                currentMission = null;

                if (Players.Count > 0)
                {
                    Players.ForEach(player =>
                    {
                        if (player.Character.Exists())
                            Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, false);
                    });

                    Players.Clear();
                }

                API.RemoveAnimDict("mp_arresting");
                API.RemoveAnimDict("random@arrests@busted");
                API.RemoveAnimDict("random@arrests");

                RegisteredPeds.ForEach(ped => ped?.Dismiss());
                RegisteredVehicles.ForEach(vehicle => vehicle?.Dismiss());
                RegisteredParticles.ForEach(particle => particle?.Stop());

                if (reason != EndState.Error)
                    EventSystem.Request<bool>("mission:completed", reason == EndState.Pass, NumberTransportArrested);

                EventSystem.Request<bool>("mission:deactivate");

                foreach (Blip blip in PluginManager.Blips)
                {
                    if (blip.Exists())
                        blip.Delete();
                }

                Instance.DiscordRichPresence.Status = "On Duty";
                Instance.DiscordRichPresence.Commit();

                PluginManager.Blips.Clear();

                DetachMissionUpdateTick();

                currentMissionData = null;
            }
            catch(Exception ex)
            {
                Logger.Debug($"Mission.Stop {ex}");
            }
        }

        internal static void DetachMissionUpdateTick()
        {
            Instance.DetachTickHandler(OnMissionUpdateTick);
        }

        internal static void AttachMissionUpdateTick()
        {
            Instance.AttachTickHandler(OnMissionUpdateTick);
        }

        /// <summary>
        /// Fails the mission for the specified reason
        /// </summary>
        /// <param name="failReason">The reason the mission failed</param>
        public async void Fail(string failReason)
        {
            if (isEndingMission) return;
            isEndingMission = true;

            MissionInfo info = Functions.GetMissionInfo(missionType);

            if (info == null) return;

            API.PlayMissionCompleteAudio("GENERIC_FAILED");

            while (!API.IsMissionCompletePlaying()) await BaseScript.Delay(0);

            if (info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Failed", failReason);
            else if (info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Setup Failed", failReason);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Mission Failed", failReason);

            Stop(EndState.Fail);
        }

        public async void Pass()
        {
            if (isEndingMission) return;
            isEndingMission = true;

            MissionInfo info = Functions.GetMissionInfo(missionType);

            if (info == null) return;

            API.PlayMissionCompleteAudio("FRANKLIN_BIG_01");

            while (!API.IsMissionCompletePlaying()) await BaseScript.Delay(0);

            if (info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Passed", info.displayName);
            else if (info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Setup Passed", info.displayName);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Mission Passed", info.displayName);

            Stop(EndState.Pass);
        }

        public static async Task OnMissionUpdateTick()
        {
            if (DateTime.Now.Subtract(LastUpdate).TotalSeconds < 5)
            {
                await BaseScript.Delay(500);
                return;
            };

            LastUpdate = DateTime.Now.AddSeconds(5);

            currentMissionData = await EventSystem.Request<MissionData>("mission:get:data", Game.Player.ServerId);

            if (currentMissionData == null)
            {
                Instance.DetachTickHandler(OnMissionUpdateTick);
                return;
            }

            // Update player information for those in the mission

            UpdateMissionPlayers(currentMissionData.PartyMembers);
            UpdateMissionPeds(currentMissionData.NetworkPeds);
            UpdateMissionVehicles(currentMissionData.NetworkVehicles);

            if (currentMissionData.IsCompleted)
            {
                RegisteredPeds.ForEach(ped => ped?.Dismiss());
                RegisteredVehicles.ForEach(vehicle => vehicle?.Dismiss());
                RegisteredParticles.ForEach(particle => particle?.Stop());

                Instance.DetachTickHandler(OnMissionUpdateTick);
            }

            Logger.Debug($"{currentMissionData}");
        }

        private static void UpdateMissionVehicles(Dictionary<int, MissionDataVehicle> networkVehicles)
        {
            if (currentMission != null)
                return;

            try
            {
                foreach (KeyValuePair<int, MissionDataVehicle> keyValuePair in networkVehicles)
                {
                    bool found = false;
                    RegisteredVehicles.ForEach(veh =>
                    {
                        // check if the vehicle is registered
                        found = (veh.NetworkId == keyValuePair.Key);
                    });

                    // if its not registered then set up the veh
                    if (!found)
                    {
                        int entityId = API.NetworkGetEntityFromNetworkId(keyValuePair.Key);
                        CitizenFX.Core.Vehicle cfxVehicle = new CitizenFX.Core.Vehicle(entityId);

                        if (cfxVehicle != null)
                        {
                            MissionDataVehicle mdv = keyValuePair.Value;

                            Logger.Debug($"{mdv}");

                            Vehicle curVehicle = new Vehicle(cfxVehicle);
                            curVehicle.IsMission = true;
                            curVehicle.IsTowable = mdv.IsTowable;

                            if (mdv.AttachBlip)
                            {
                                Blip b = curVehicle.AttachBlip();
                                b.Color = BlipColor.Red;
                                b.Scale = .5f;
                            }

                            RegisteredVehicles.Add(curVehicle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"UpdateMissionVehicles -> {ex}");
            }
        }

        private static void UpdateMissionPeds(Dictionary<int, MissionDataPed> networkPeds)
        {
            // Need to monitor if a ped was arrested and update as required, add requirements to the server for best results
            // this way the completion and success comes from the server and not from the client.

            if (currentMission != null)
                return;

            try
            {

                foreach (KeyValuePair<int, MissionDataPed> keyValuePair in networkPeds)
                {
                    bool found = false;
                    RegisteredPeds.ForEach(ped =>
                    {
                    // check if the ped is registered
                    found = (ped.NetworkId == keyValuePair.Key);
                    });

                    // if they are not registered then set up the ped
                    if (!found)
                    {
                        MissionDataPed mdp = keyValuePair.Value;

                        int entityId = API.NetworkGetEntityFromNetworkId(keyValuePair.Key);
                        CitizenFX.Core.Ped cfxPed = new CitizenFX.Core.Ped(entityId);

                        if (cfxPed != null)
                        {
                            Ped curPed = new Ped(cfxPed);
                            RegisteredPeds.Add(curPed);

                            curPed.IsSuspect = mdp.IsSuspect;
                            curPed.IsMission = true;

                            Logger.Debug($"{mdp}");

                            if (mdp.AttachBlip)
                            {
                                Blip b = curPed.AttachBlip();
                                b.Sprite = BlipSprite.Enemy;
                                b.Color = BlipColor.Red;
                                b.Scale = .5f;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"UpdateMissionPeds -> {ex}");
            }
        }

        private static void UpdateMissionPlayers(List<int> partyMembers)
        {
            try
            {
                partyMembers.ForEach(memberServerID =>
                {
                    if (memberServerID == Game.Player.ServerId) return;

                    Player player = new Player(memberServerID);

                    if (Players.Contains(player)) return;

                    if (player != null)
                    {
                        AddPlayer(player);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"UpdateMissionPlayers -> {ex}");
            }
        }

        public void DiscordStatus(string status)
        {
            Instance.DiscordRichPresence.Status = status;
            Instance.DiscordRichPresence.Commit();
        }
    }
}
