﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using NativeUI;
using System;
using System.Collections.Generic;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client
{
    public abstract class Mission
    {
        internal static List<Type> missions = new List<Type>();
        internal static bool isOnMission = false;
        internal static Mission currentMission = null;
        internal static Type missionType = null;

        public static List<Player> Players { get; internal set; }

        public static List<Vehicle> RegisteredVehicles { get; internal set; }
        public static List<Ped> RegisteredPeds { get; internal set; }
        public static List<ParticleEffect> RegisteredParticles { get; internal set; }

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

        /// <summary>
        /// Stops a mission
        /// </summary>
        /// <param name="reason">The reason the mission was stopped</param>
        public void Stop(EndState reason)
        {
            isOnMission = false;
            missionType = null;

            currentMission?.End();

            currentMission = null;

            switch(reason)
            {
                case EndState.Pass:
                    break;
                case EndState.Error:
                    break;
            }

            if (Players.Count > 0)
            {
                Players.ForEach(player =>
                {
                    if (player.Character.Exists())
                        Decorators.Set(player.Character.Handle, Decorators.PLAYER_ASSISTING, false);
                });

                Players.Clear();
            }

            RegisteredPeds.ForEach(ped => ped?.Dismiss());
            RegisteredVehicles.ForEach(vehicle => vehicle?.Dismiss());
            RegisteredParticles.ForEach(particle => particle?.Stop());

            foreach (Blip blip in PluginManager.Blips)
            {
                if (blip.Exists())
                    blip.Delete();
            }

            PluginManager.Blips.Clear();
        }

        /// <summary>
        /// Fails the mission for the specified reason
        /// </summary>
        /// <param name="failReason">The reason the mission failed</param>
        public async void Fail(string failReason)
        {
            MissionInfo info = Functions.GetMissionInfo(missionType);

            if (info == null) return;

            Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "GENERIC_FAILED");
            while (!Function.Call<bool>(Hash.IS_MISSION_COMPLETE_PLAYING)) await BaseScript.Delay(0);
            if (info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Failed", failReason);
            else if (info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Heist Setup Failed", failReason);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~r~Mission Failed", failReason);

            Stop(EndState.Fail);
        }

        public async void Pass()
        {
            MissionInfo info = Functions.GetMissionInfo(missionType);

            if (info == null) return;

            API.PlayMissionCompleteAudio("FRANKLIN_BIG_01");

            while (!API.IsMissionCompletePlaying())
            {
                await BaseScript.Delay(0);
            }

            if (info.type == MissionType.Heist) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Passed", info.displayName);
            else if (info.type == MissionType.HeistSetup) BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Heist Setup Passed", info.displayName);
            else BigMessageThread.MessageInstance.ShowSimpleShard($"~y~Mission Passed", info.displayName);

            Stop(EndState.Pass);
        }
    }

    /// <summary>
    /// The reason a mission ended, this is exclusively used for the Stop function
    /// </summary>
    public enum EndState
    {
        Fail,
        Pass,
        Error
    }
}
