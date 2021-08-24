using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
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

        public int TotalSprinting = 0;
        public int TotalSwiming = 0;

        LevelManager levelManager;

        DateTime DEFAULT = new DateTime(1970, 01, 01);

        DateTime sprintStart = new DateTime(1970, 01, 01);
        DateTime sprintEnd = new DateTime(1970, 01, 01);

        DateTime swimStart = new DateTime(1970, 01, 01);
        DateTime swimEnd = new DateTime(1970, 01, 01);

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
                sprintStart = DateTime.Now;
            }
            else if (!Game.PlayerPed.IsSprinting && wasSprinting)
            {
                wasSprinting = false;
                sprintEnd = DateTime.Now;
                // Log Duration
                double secondsSprinting = sprintEnd.Subtract(sprintStart).TotalSeconds;
                // Send data back, update server, get total and find level with total, this then = Stamina Stat
                int currentLevel = levelManager.GetLevelForXP(TotalSprinting, MAX_EXP, MAX_LEVEL);
                int currentTotal = TotalSprinting;
                int storedTotalSprinting = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_SPRINTING, secondsSprinting);
                TotalSprinting = storedTotalSprinting;

                if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsSprinting), MAX_EXP, MAX_LEVEL))
                {
                    NotificationManager nm = NotificationManager.GetModule();
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
                swimStart = DateTime.Now;
            }
            else if (!Game.PlayerPed.IsSwimmingUnderWater && wasSwimming && !PlayerOptionsManager.GetModule().IsScubaGearEnabled)
            {
                wasSwimming = false;
                swimEnd = DateTime.Now;

                double secondsTotalSwiming = swimEnd.Subtract(swimStart).TotalSeconds;

                int currentLevel = levelManager.GetLevelForXP(TotalSwiming, MAX_EXP, MAX_LEVEL);
                int currentTotal = TotalSwiming;
                // Send data back, update server, get total and find level with total, this then = Breathing Stat
                int storedTotalSwiming = await EventSystem.Request<int>("character:update:stat:timed", Stat.STAT_SWIMMING, secondsTotalSwiming);
                TotalSwiming = storedTotalSwiming;

                if (levelManager.AddExp(currentLevel, currentTotal, (int)(secondsTotalSwiming), MAX_EXP, MAX_LEVEL))
                {
                    NotificationManager nm = NotificationManager.GetModule();
                    int newLevel = levelManager.GetLevelForXP(TotalSprinting, MAX_EXP, MAX_LEVEL);
                    UpdateStat(Cache.Character.MP0_LUNG_CAPACITY, "Breathing Increased", newLevel);

                } // TODO SETUP ON LOAD

                Logger.Debug($"Duration Swam: {secondsTotalSwiming:0.00}. currentLvl: {currentLevel}/{TotalSwiming}/{storedTotalSwiming}");

                swimStart = DEFAULT;
                swimEnd = DEFAULT;
            }
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
