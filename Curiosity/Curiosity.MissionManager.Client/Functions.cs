using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Environment.Entities.Models;

namespace Curiosity.MissionManager.Client
{
    public class Functions
    {
        static PluginManager Instance => PluginManager.Instance;

        static NUIMarker questMarker1;
        static int _soundId = -1;
        static Vector3 _start = new Vector3(-543.9988f, -157.8393f, 38.54123f);
        static Vector3 _scale = new Vector3(1.2f, 1.2f, 1f);
        static Type HalloweenMission;
        static Blip HalloweenBlip;

        /// <summary>
        /// Registers a mission so it can be seen and used in-game
        /// </summary>
        /// <param name="mission">The mission to register</param>
        public static async void RegisterMission(Type mission)
        {
            if (mission.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(MissionInfo)) == null)
                throw new Exception("Mission must have the MissionInfo attribute!");

            MissionInfo missionInfo = GetMissionInfo(mission);

            // Logger.Info($"[MissionManager] Registered: {mission.Name} | {missionInfo.missionType} ({missionInfo.xPos}, {missionInfo.yPos}, {missionInfo.zPos}) {missionInfo.startPoint != Vector3.Zero}");

            if (missionInfo.startPoint != Vector3.Zero)
            {
                bool isHalloween = await EventSystem.EventSystem.Request<bool>("weather:is:halloween");
                bool hasCompletedQuest = false; //await EventSystem.EventSystem.Request<bool>("mission:quest:completed", 1);

#if DEBUG
                isHalloween = true;
                hasCompletedQuest = false;
#endif

                if (missionInfo.missionType == MissionType.Halloween && isHalloween && !hasCompletedQuest)
                {
                    questMarker1 = new NUIMarker(MarkerType.VerticalCylinder, _start, _scale, 2f, Color.FromArgb(255, 120, 0, 0), placeOnGround: true);
                    Mission.AddMarker("start", questMarker1);
                    HalloweenMission = mission;
                    Instance.AttachTickHandler(OnCustomMissionStart);
                    Notify.Info($"There is a phone booth thats ringing, maybe you should answer it?", "top-right");

                    string textureDictionary = "halloween";
                    long textureDict = CreateRuntimeTxd(textureDictionary);
                    string textureName = "phoneBooth";

                    CreateRuntimeTextureFromImage(textureDict, textureName, "assets/images/phoneBoothHalloween.png");

                    BlipMissionInfo blipMissionInfo = new BlipMissionInfo();
                    blipMissionInfo.Title = "Happy Halloween!";
                    blipMissionInfo.TextureDictionary = textureDictionary;
                    blipMissionInfo.TextureName = textureName;

                    HalloweenBlip = BlipManager.GetModule().AddBlip("Halloween Phone Call", (BlipSprite)437, BlipColor.Red, _start, blipMissionInfo);

                }
                // world blips for set mission locations
            }

            if (missionInfo.missionType != MissionType.Halloween)
                Mission.missions.Add(mission);
        }

        private static async Task OnCustomMissionStart()
        {
            if (questMarker1 is null)
            {
                Instance.DetachTickHandler(OnCustomMissionStart);
                return;
            }

            if (Game.PlayerPed.Position.Distance(_start) > 50f)
            {
                if (_soundId != -1)
                {
                    StopSound(_soundId);
                    ReleaseSoundId(_soundId);
                    _soundId = -1;
                }

                await BaseScript.Delay(500);
                return;
            }

            if (Game.PlayerPed.Position.Distance(_start) < 4f)
            {
                if (_soundId == -1)
                {
                    _soundId = GetSoundId();
                    PlaySoundFrontend(_soundId, "Remote_Ring", "Phone_SoundSet_Michael", true);
                    Logger.Info($"_soundId: {_soundId}");
                }
            }

            if (Game.PlayerPed.Position.Distance(_start) > 4f && _soundId != -1)
            {
                StopSound(_soundId);
                ReleaseSoundId(_soundId);
                _soundId = -1;
            }

            if (questMarker1.IsInMarker && !Mission.isOnMission && !Game.PlayerPed.IsInVehicle())
            {
                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to answer the phone.");
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    StartMission(HalloweenMission, "Answering a phone call....");
                    
                    StopSound(_soundId);
                    ReleaseSoundId(_soundId);
                    _soundId = -1;

                    await BaseScript.Delay(1000);

                    _soundId = GetSoundId();
                    PlaySoundFrontend(_soundId, "Hang_Up", "Phone_SoundSet_Michael", true);
                    await BaseScript.Delay(1000);
                    StopSound(_soundId);
                    ReleaseSoundId(_soundId);
                    _soundId = -1;

                    BlipManager.GetModule().RemoveBlip(HalloweenBlip);
                }
            }
            else if (questMarker1.IsInRange && Mission.isOnMission)
            {
                Screen.DisplayHelpTextThisFrame($"Cannot answer the phone while on a callout.");
            }
            else if (questMarker1.IsInRange && Game.PlayerPed.IsInVehicle())
            {
                Screen.DisplayHelpTextThisFrame($"Must leave your vehicle to answer the phone.");
            }
        }

        /// <summary>
        /// Starts a mission
        /// </summary>
        /// <param name="mission">The mission to start</param>
        public static void StartMission(Type mission, string discordStatus = "Responding to a call")
        {
            Logger.Info($"Started Mission");

            // Remove any blips if they are left around
            foreach (Blip blip in PluginManager.Blips)
            {
                blip.Delete();
            }

            var constructor = mission.GetConstructor(Type.EmptyTypes);
            var typeClass = constructor.Invoke(new object[] { });
            var mis = (Mission)typeClass;

            MissionInfo missionInfo = GetMissionInfo(mission);

            Logger.Debug($"StartMission : {missionInfo.displayName}");

            Instance.DiscordRichPresence.Status = discordStatus;
            Instance.DiscordRichPresence.Commit();

            Mission.currentMission = mis;
            Mission.missionType = mission;
            Mission.AddPlayer(Game.Player);
            Mission.currentMissionType = missionInfo.missionType;
            Mission.NumberPedsArrested = 0;
            
            Mission.PatrolZone = JobManager.PatrolZone; // Always get the current zone from the player at this point
            // Mission randomiser will also use the PatrolZone of the player to select a mission, but the mission needs to know the players state IF the mission doesn't have a PatrolZone assigned

            if (Mission.RegisteredPeds.Count > 0)
            {
                Mission.RegisteredPeds.Clear();
                Mission.RegisteredPeds = null;
            }

            if (Mission.RegisteredVehicles.Count > 0)
            {
                Mission.RegisteredVehicles.Clear();
                Mission.RegisteredVehicles = null;
            }

            if (Mission.RegisteredParticles.Count > 0)
            {
                Mission.RegisteredParticles.Clear();
                Mission.RegisteredParticles = null;
            }

            Mission.RegisteredPeds = new List<Classes.Ped>();
            Mission.RegisteredVehicles = new List<Classes.Vehicle>();
            Mission.RegisteredParticles = new List<ParticleEffect>();

            API.RequestAnimDict("mp_arresting");
            API.RequestAnimDict("random@arrests@busted");
            API.RequestAnimDict("random@arrests");

            Mission.isOnMission = true;
            Mission.isMessagingServer = false;
            Mission.isEndingMission = false;

            Mission.AttachMissionUpdateTick();

            Mission.currentMission.Start();
        }

        /// <summary>
        /// Returns a MissionInfo object based on the inputted mission's attributes
        /// </summary>
        /// <param name="mission">The mission to get info on</param>
        internal static MissionInfo GetMissionInfo(Type mission)
        {
            if (mission == null) return null;

            MissionInfo info = (MissionInfo)mission.GetCustomAttribute(typeof(MissionInfo));
            return info;
        }

        public static Blip SetupLocationBlip(Vector3 location, BlipSprite blipSprite = BlipSprite.BigCircle, float scale = 0.5f, BlipColor blipColor = BlipColor.Yellow)
        {
            Blip locationBlip = World.CreateBlip(location);
            locationBlip.Sprite = blipSprite;
            locationBlip.Scale = scale;
            locationBlip.Color = blipColor;
            locationBlip.Alpha = 126;
            locationBlip.ShowRoute = true;
            locationBlip.Priority = 9;
            locationBlip.IsShortRange = true;
            locationBlip.Name = "Mission Area";

            Notify.Dispatch("~g~GPS Updated", $"Please travel to the location shown on your ~b~GPS~s~.");

            API.SetBlipDisplay(locationBlip.Handle, 5);

            return locationBlip;
        }

        public static void PlayScannerAudio(string audio)
        {

        }
    }
}
