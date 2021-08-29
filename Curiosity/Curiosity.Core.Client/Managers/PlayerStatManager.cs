using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Enums;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerStatManager : Manager<PlayerStatManager>
    {
        public const int MAX_LEVEL = 100;
        public const int MAX_EXP = 14391160;

        bool isSetup = false;

        bool wasSprinting = false;
        bool wasSwimming = false;
        bool wasDriving = false;
        bool wasFlying = false;

        public int TotalSprinting = 0;
        public int TotalSwiming = 0;
        public int TotalDriving = 0;
        public int TotalFlying = 0;

        LevelManager levelManager;

        DateTime DEFAULT = new DateTime(1970, 01, 01);

        DateTime sprintStart = new DateTime(1970, 01, 01);
        DateTime sprintEnd = new DateTime(1970, 01, 01);

        DateTime swimStart = new DateTime(1970, 01, 01);
        DateTime swimEnd = new DateTime(1970, 01, 01);

        DateTime drivingStart = new DateTime(1970, 01, 01);
        DateTime drivingEnd = new DateTime(1970, 01, 01);

        DateTime flyingStart = new DateTime(1970, 01, 01);
        DateTime flyingEnd = new DateTime(1970, 01, 01);

        public override void Begin()
        {
            levelManager = LevelManager.GetModule();
        }

        [TickHandler(SessionWait = true)]
        private async Task OnPlayerStateTask()
        {
            if (Game.PlayerPed.IsSprinting && !wasSprinting)
            {
                wasSprinting = true;
                sprintStart = DateTime.UtcNow;

                while (Game.PlayerPed.IsSprinting)
                {
                    await BaseScript.Delay(100);
                }
            }
            else if (!Game.PlayerPed.IsSprinting && wasSprinting)
            {
                wasSprinting = false;
                sprintEnd = DateTime.UtcNow;
                // Log Duration
                double secondsSprinting = sprintEnd.Subtract(sprintStart).TotalSeconds;
                // Send data back, update server, get total and find level with total, this then = Stamina Stat
                int currentLevel = levelManager.GetLevelForXP(TotalSprinting, MAX_EXP, MAX_LEVEL);
                int currentTotal = TotalSprinting;
                int storedTotalSprinting = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_SPRINTING, secondsSprinting);
                TotalSprinting = storedTotalSprinting;

                if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsSprinting), MAX_EXP, MAX_LEVEL))
                {
                    int newLevel = levelManager.GetLevelForXP(TotalSprinting, MAX_EXP, MAX_LEVEL);
                    UpdateStat(Cache.Character.MP0_STAMINA, "Sprinting Increased", newLevel);

                } // TODO SETUP ON LOAD

                Logger.Debug($"Duration Sprinted: {secondsSprinting:0.00}, currentLvl: {currentLevel}/{TotalSprinting}/{storedTotalSprinting}");

                // Reset
                sprintStart = DEFAULT;
                sprintEnd = DEFAULT;
            }

            if (Game.PlayerPed.IsSwimmingUnderWater && !wasSwimming)
            {
                wasSwimming = true;
                swimStart = DateTime.UtcNow;

                while (Game.PlayerPed.IsSwimmingUnderWater)
                {
                    await BaseScript.Delay(100);
                }
            }
            else if (!Game.PlayerPed.IsSwimmingUnderWater && wasSwimming && !PlayerOptionsManager.GetModule().IsScubaGearEnabled)
            {
                wasSwimming = false;
                swimEnd = DateTime.UtcNow;

                double secondsTotalSwiming = swimEnd.Subtract(swimStart).TotalSeconds;

                int currentLevel = levelManager.GetLevelForXP(TotalSwiming, MAX_EXP, MAX_LEVEL);
                int currentTotal = TotalSwiming;
                // Send data back, update server, get total and find level with total, this then = Breathing Stat
                int storedTotalSwiming = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_SWIMMING, secondsTotalSwiming);
                TotalSwiming = storedTotalSwiming;

                if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsTotalSwiming), MAX_EXP, MAX_LEVEL))
                {
                    int newLevel = levelManager.GetLevelForXP(TotalSwiming, MAX_EXP, MAX_LEVEL);
                    UpdateStat(Cache.Character.MP0_LUNG_CAPACITY, "Breathing Increased", newLevel);

                } // TODO SETUP ON LOAD

                Logger.Debug($"Duration Swam: {secondsTotalSwiming:0.00}. currentLvl: {currentLevel}/{TotalSwiming}/{storedTotalSwiming}");

                swimStart = DEFAULT;
                swimEnd = DEFAULT;
            }

            if (Cache.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Cache.PlayerPed.CurrentVehicle;

                if (vehicle.Driver == Cache.PlayerPed && (vehicle.Model.IsCar || vehicle.Model.IsBike))
                {
                    float speed = vehicle.Speed * 3.6f;
                    if (speed > 30 && !wasDriving)
                    {
                        wasDriving = true;
                        drivingStart = DateTime.UtcNow;

                        while (speed > 30)
                        {
                            speed = vehicle.Speed * 3.6f;
                            await BaseScript.Delay(100);
                        }
                    }
                    else if (speed < 30 && wasDriving)
                    {
                        wasDriving = false;
                        drivingEnd = DateTime.UtcNow;

                        double secondsTotalDriving = drivingEnd.Subtract(drivingStart).TotalSeconds;

                        int currentLevel = levelManager.GetLevelForXP(TotalDriving, MAX_EXP, MAX_LEVEL);
                        int currentTotal = TotalDriving;
                        // Send data back, update server, get total and find level with total, this then = Breathing Stat
                        int storedTotalDriving = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_WHEELIE_ABILITY, secondsTotalDriving);
                        TotalDriving = storedTotalDriving;

                        if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsTotalDriving), MAX_EXP, MAX_LEVEL))
                        {
                            int newLevel = levelManager.GetLevelForXP(TotalDriving, MAX_EXP, MAX_LEVEL);
                            UpdateStat(Cache.Character.MP0_WHEELIE_ABILITY, "Driving Increased", newLevel);

                        } // TODO SETUP ON LOAD

                        Logger.Debug($"Duration Driving: {secondsTotalDriving:0.00}. currentLvl: {currentLevel}/{TotalDriving}/{storedTotalDriving}");

                        drivingStart = DEFAULT;
                        drivingEnd = DEFAULT;
                    }
                }
                
                if (vehicle.Driver == Cache.PlayerPed && (vehicle.Model.IsPlane || vehicle.Model.IsHelicopter))
                {
                    float speed = vehicle.Speed * 3.6f;
                    if (speed > 10 && !wasFlying && vehicle.IsInAir)
                    {
                        wasFlying = true;
                        flyingStart = DateTime.UtcNow;
                        
                        while (speed > 10 && vehicle.IsInAir)
                        {
                            speed = vehicle.Speed * 3.6f;
                            await BaseScript.Delay(100);
                        }
                    }
                    else if (speed < 10 && wasFlying && !vehicle.IsInAir)
                    {
                        wasFlying = false;
                        flyingEnd = DateTime.UtcNow;

                        double secondsTotalFlying = flyingEnd.Subtract(flyingStart).TotalSeconds;

                        int currentLevel = levelManager.GetLevelForXP(TotalFlying, MAX_EXP, MAX_LEVEL);
                        int currentTotal = TotalFlying;
                        // Send data back, update server, get total and find level with total, this then = Breathing Stat
                        int storedTotalFlying = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_FLYING_ABILITY, secondsTotalFlying);
                        TotalFlying = storedTotalFlying;

                        if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsTotalFlying), MAX_EXP, MAX_LEVEL))
                        {
                            int newLevel = levelManager.GetLevelForXP(TotalFlying, MAX_EXP, MAX_LEVEL);
                            UpdateStat(Cache.Character.MP0_FLYING_ABILITY, "Flying Increased", newLevel);

                        } // TODO SETUP ON LOAD

                        Logger.Debug($"Duration Flying: {secondsTotalFlying:0.00}. currentLvl: {currentLevel}/{TotalFlying}/{storedTotalFlying}");

                        flyingStart = DEFAULT;
                        flyingEnd = DEFAULT;
                    }
                }
            }

            await BaseScript.Delay(500);
        }

        void UpdateStat(string stat, string message, int newLevel)
        {
            uint hash = (uint)API.GetHashKey(stat);

            int currentStatValue = 0;
            API.StatGetInt(hash, ref currentStatValue, -1);
            
            SetStatValue(newLevel, hash);

            int newStatValue = 0;
            API.StatGetInt(hash, ref newStatValue, -1);

            if (currentStatValue != newStatValue)
            {
                NativeUI.Notifications.ShowStatNotification(newStatValue, currentStatValue, message);
                NotificationManager.GetModule().Success(message);
            }
        }

        public void SetStatValue(int newLevel, uint hash)
        {
            if (newLevel < 10)
            {
                API.StatSetInt(hash, 0, true);
            }

            if (newLevel >= 10 && newLevel < 20)
            {
                API.StatSetInt(hash, 20, true);
            }

            if (newLevel >= 20 && newLevel < 30)
            {
                API.StatSetInt(hash, 40, true);
            }

            if (newLevel >= 30 && newLevel < 40)
            {
                API.StatSetInt(hash, 60, true);
            }

            if (newLevel >= 40 && newLevel < 50)
            {
                API.StatSetInt(hash, 80, true);
            }

            if (newLevel >= 50 && newLevel < 60)
            {
                API.StatSetInt(hash, 100, true);
            }

            if (newLevel >= 60)
            {
                API.StatSetInt(hash, 100, true);
            }
        }
    }
}
