using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using static Curiosity.Shared.Client.net.Helper.ControlHelper;

namespace Curiosity.Levels.Client.net
{
    /// <summary>
    /// Pretty much : https://github.com/VenomXNL/XNLRankBar
    /// </summary>
    public class RankBar : BaseScript
    {
        const int HUD_SCALEFORM_ID = 19;

        List<int> defaultRanks = new List<int>();

        bool useRedBarWhenLosingXP = true;
        int maxPlayerLevel = 500;
        bool enableZKeyForRankbar = true;

        int currentLevel = 0;
        int currentXP = 0;

        public RankBar()
        {
            SetupRanks();

            EventHandlers["curiosity:Client:Rank:SetInitialXpLevels"] += new Action<int, bool, bool>(SetInitialXpLevels);
            EventHandlers["curiosity:Client:Rank:AddPlayerXP"] += new Action<int>(AddPlayerXP);
            EventHandlers["curiosity:Client:Rank:RemovePlayerXP"] += new Action<int>(RemovePlayerXP);

            Tick += CheckButton;
        }

        void AddPlayerXP(int xpAmount)
        {
            if (xpAmount < 0)
                return;

            int internalCurrentLevel = GetLevelFromXP(currentXP);
            int currentXPWithAddedXP = currentXP + xpAmount;
            int newLevel = GetLevelFromXP(currentXPWithAddedXP);
            int levelDifference = 0;

            if (newLevel > maxPlayerLevel - 1)
            {
                newLevel = maxPlayerLevel - 1;
                currentXPWithAddedXP = GetXPCeilingForLevel(maxPlayerLevel - 1);
            }

            if (newLevel > internalCurrentLevel)
            {
                levelDifference = (newLevel - internalCurrentLevel);
            }

            if (levelDifference > 0)
            {
                int startAtLevel = internalCurrentLevel;
                CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), currentXP, currentXPWithAddedXP, startAtLevel, false);

                for (int i = 0; i <= levelDifference; i++)
                {
                    if (i == levelDifference)
                    {
                        CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), GetXPFloorForLevel(startAtLevel), currentXPWithAddedXP, startAtLevel, false);
                    }
                    else
                    {
                        CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), startAtLevel, false);
                    }
                    startAtLevel++;
                }
            }
            else
            {
                CreateRankBar(GetXPFloorForLevel(newLevel), GetXPCeilingForLevel(newLevel), currentXP, currentXPWithAddedXP, newLevel, false);
            }

            currentXP = currentXPWithAddedXP;

            if (levelDifference > 0)
            {
                // do something with a level up?
            }
        }

        void RemovePlayerXP(int xpAmount)
        {
            if (xpAmount < 0)
                return;

            int internalCurrentLevel = GetLevelFromXP(currentXP);
            int currentXPWithRemovedXP = currentXP - xpAmount;
            int newLevel = GetLevelFromXP(currentXPWithRemovedXP);
            int levelDifference = 0;

            if (newLevel < 1)
            {
                newLevel = 1;
            }

            if (currentXPWithRemovedXP < 0)
            {
                currentXPWithRemovedXP = 0;
            }

            if (newLevel < internalCurrentLevel)
            {
                levelDifference = Math.Abs(newLevel - internalCurrentLevel);
            }

            if (levelDifference > 0)
            {
                int startAtLevel = internalCurrentLevel;
                CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), currentXP, currentXPWithRemovedXP, startAtLevel, true);

                for (int i = 0; i <= levelDifference; i++)
                { 
                    if (i == levelDifference)
                    {
                        CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), GetXPFloorForLevel(startAtLevel), currentXPWithRemovedXP, startAtLevel, true);
                    }
                    else
                    {
                        CreateRankBar(GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), GetXPFloorForLevel(startAtLevel), GetXPCeilingForLevel(startAtLevel), startAtLevel, true);
                    }
                    startAtLevel++;
                }
            }
            else
            {
                CreateRankBar(GetXPFloorForLevel(newLevel), GetXPCeilingForLevel(newLevel), currentXP, currentXPWithRemovedXP, newLevel, true);
            }

            currentXP = currentXPWithRemovedXP;

            if (levelDifference > 0)
            {
                // do something with a level down?
            }
        }

        void SetInitialXpLevels(int netCurrentXP, bool netShowRankbar, bool netShowRankbarAnimating)
        {
            currentXP = netCurrentXP;

            if (netShowRankbar)
            {
                int currentLevel = GetLevelFromXP(currentXP);
                int animateFrom = currentXP;
                if (netShowRankbarAnimating)
                {
                    animateFrom = GetXPFloorForLevel(currentLevel);
                }
                CreateRankBar(GetXPFloorForLevel(currentLevel), GetXPCeilingForLevel(currentLevel), animateFrom, currentXP, currentLevel, false);
            }
        }

        async Task CheckButton()
        {
            if (!enableZKeyForRankbar)
                return;

            while (true)
            {
                if (IsControlPressed(Control.MultiplayerInfo))
                {
                    int currentLevel = GetLevelFromXP(currentXP);
                    CreateRankBar(GetXPFloorForLevel(currentLevel), GetXPCeilingForLevel(currentLevel), currentXP, currentXP, currentLevel, false);
                }

                await Delay(0);
            }
        }

        int GetXPFloorForLevel(int currentLevel)
        {
            if (currentLevel > 7999)
                currentLevel = 7999;

            if (currentLevel < 2)
                return 0;

            if (currentLevel > 100)
            {
                int baseXp = defaultRanks[defaultRanks.Count - 1];
                int extraAddPerLevel = 50;
                int mainAddPerLevel = 28550;

                int baseLevel = currentLevel - defaultRanks.Count;
                int currentXpNeeded = 0;
                for(int i = 0; i < baseLevel; i++)
                {
                    mainAddPerLevel = mainAddPerLevel + extraAddPerLevel;
                    currentXpNeeded = currentXpNeeded + mainAddPerLevel;
                }

                return baseXp + currentXpNeeded;
            }
            return defaultRanks[currentLevel - 1];
        }

        int GetXPCeilingForLevel(int currentLevel)
        {
            if (currentLevel > 7999)
                currentLevel = 7999;

            if (currentLevel < 1)
                return 800;

            if (currentLevel > 99)
            {
                int baseXp = defaultRanks[defaultRanks.Count - 1];
                int extraAddPerLevel = 50;
                int mainAddPerLevel = 28550;

                int baseLevel = currentLevel - (defaultRanks.Count - 1);
                int currentXpNeeded = 0;
                for (int i = 0; i < baseLevel; i++)
                {
                    mainAddPerLevel = mainAddPerLevel + extraAddPerLevel;
                    currentXpNeeded = currentXpNeeded + mainAddPerLevel;
                }

                return baseXp + currentXpNeeded;
            }
            return defaultRanks[currentLevel - 1];
        }

        int GetLevelFromXP(int currentXP)
        {
            int searchingFor = currentXP;

            if (searchingFor < 0)
                return 1;

            if (searchingFor < defaultRanks[defaultRanks.Count - 1])
            {
                int currentLevelScan = 0;

                foreach(int xp in defaultRanks)
                {
                    currentLevelScan = currentLevelScan + 1;
                    if (searchingFor < xp)
                        break;
                }
                return currentLevelScan;
            }
            else
            {
                int baseXp = defaultRanks[defaultRanks.Count - 1];
                int extraAddPerLevel = 50;
                int mainAddPerLevel = 28550;

                int currentXpNeeded = 0;
                int currentLevelFound = -1;

                for(int i = 0; i < (maxPlayerLevel - (defaultRanks.Count - 1)); i++)
                {
                    mainAddPerLevel = mainAddPerLevel + extraAddPerLevel;
                    currentXpNeeded = currentXpNeeded + mainAddPerLevel;
                    currentLevelFound = i;
                    if (searchingFor < (baseXp + currentXpNeeded))
                        break;
                }
                return currentLevelFound + defaultRanks.Count;
            }
        }

        async void CreateRankBar(int xpStartLimitRankBar, int xpEndLimitRankBar, int playersPreviousXP, int playersCurrentXP, int currentPlayerLevel, bool takingAwayXP)
        {
            int rankBarColor = 116; // HUD_COLOR on GT-MP

            if (takingAwayXP && useRedBarWhenLosingXP)
            {
                rankBarColor = 6;
            }

            if (!API.HasHudScaleformLoaded(HUD_SCALEFORM_ID))
            {
                API.RequestHudScaleform(HUD_SCALEFORM_ID);
                while (!API.HasHudScaleformLoaded(HUD_SCALEFORM_ID))
                {
                    await Delay(1);
                }
            }

            API.BeginScaleformMovieMethodHudComponent(HUD_SCALEFORM_ID, "SET_COLOUR");
            API.PushScaleformMovieFunctionParameterInt(rankBarColor);
            API.EndScaleformMovieMethodReturn();

            API.BeginScaleformMovieMethodHudComponent(HUD_SCALEFORM_ID, "SET_RANK_SCORES");
            API.PushScaleformMovieFunctionParameterInt(xpStartLimitRankBar);
            API.PushScaleformMovieFunctionParameterInt(xpEndLimitRankBar);
            API.PushScaleformMovieFunctionParameterInt(playersPreviousXP);
            API.PushScaleformMovieFunctionParameterInt(playersCurrentXP);
            API.PushScaleformMovieFunctionParameterInt(currentPlayerLevel);
            API.PushScaleformMovieFunctionParameterInt(100);
            API.EndScaleformMovieMethodReturn();
        }

        void SetupRanks()
        {
            defaultRanks.Add(800);
            defaultRanks.Add(2100);
            defaultRanks.Add(3800);
            defaultRanks.Add(6100);
            defaultRanks.Add(9500);
            defaultRanks.Add(12500);
            defaultRanks.Add(16000);
            defaultRanks.Add(19800);
            defaultRanks.Add(24000);
            defaultRanks.Add(28500);
            defaultRanks.Add(33400);
            defaultRanks.Add(38700);
            defaultRanks.Add(44200);
            defaultRanks.Add(50200);
            defaultRanks.Add(56400);
            defaultRanks.Add(63000);
            defaultRanks.Add(69900);
            defaultRanks.Add(77100);
            defaultRanks.Add(84700);
            defaultRanks.Add(92500);
            defaultRanks.Add(100700);
            defaultRanks.Add(109200);
            defaultRanks.Add(118000);
            defaultRanks.Add(127100);
            defaultRanks.Add(136500);
            defaultRanks.Add(146200);
            defaultRanks.Add(156200);
            defaultRanks.Add(166500);
            defaultRanks.Add(177100);
            defaultRanks.Add(188000);
            defaultRanks.Add(199200);
            defaultRanks.Add(210700);
            defaultRanks.Add(222400);
            defaultRanks.Add(234500);
            defaultRanks.Add(246800);
            defaultRanks.Add(259400);
            defaultRanks.Add(272300);
            defaultRanks.Add(285500);
            defaultRanks.Add(299000);
            defaultRanks.Add(312700);
            defaultRanks.Add(326800);
            defaultRanks.Add(341000);
            defaultRanks.Add(355600);
            defaultRanks.Add(370500);
            defaultRanks.Add(385600);
            defaultRanks.Add(401000);
            defaultRanks.Add(416600);
            defaultRanks.Add(432600);
            defaultRanks.Add(448800);
            defaultRanks.Add(465200);
            defaultRanks.Add(482000);
            defaultRanks.Add(499000);
            defaultRanks.Add(516300);
            defaultRanks.Add(533800);
            defaultRanks.Add(551600);
            defaultRanks.Add(569600);
            defaultRanks.Add(588000);
            defaultRanks.Add(606500);
            defaultRanks.Add(625400);
            defaultRanks.Add(644500);
            defaultRanks.Add(663800);
            defaultRanks.Add(683400);
            defaultRanks.Add(703300);
            defaultRanks.Add(723400);
            defaultRanks.Add(743800);
            defaultRanks.Add(764500);
            defaultRanks.Add(785400);
            defaultRanks.Add(806500);
            defaultRanks.Add(827900);
            defaultRanks.Add(849600);
            defaultRanks.Add(871500);
            defaultRanks.Add(893600);
            defaultRanks.Add(916000);
            defaultRanks.Add(938700);
            defaultRanks.Add(961600);
            defaultRanks.Add(984700);
            defaultRanks.Add(1008100);
            defaultRanks.Add(1031800);
            defaultRanks.Add(1055700);
            defaultRanks.Add(1079800);
            defaultRanks.Add(1104200);
            defaultRanks.Add(1128800);
            defaultRanks.Add(1153700);
            defaultRanks.Add(1178800);
            defaultRanks.Add(1204200);
            defaultRanks.Add(1229800);
            defaultRanks.Add(1255600);
            defaultRanks.Add(1281700);
            defaultRanks.Add(1308100);
            defaultRanks.Add(1334600);
            defaultRanks.Add(1361400);
            defaultRanks.Add(1388500);
            defaultRanks.Add(1415800);
            defaultRanks.Add(1443300);
            defaultRanks.Add(1471100);
            defaultRanks.Add(1499100);
            defaultRanks.Add(1527300);
            defaultRanks.Add(1555800);
            defaultRanks.Add(1584350);
        }
    }
}
