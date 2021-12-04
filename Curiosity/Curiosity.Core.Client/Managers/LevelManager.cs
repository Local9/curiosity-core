using System;

namespace Curiosity.Core.Client.Managers
{
    public class LevelManager : Manager<LevelManager>
    {
        public override void Begin()
        {

        }

        public int ExperienceToNextLevel(int level)
        {
            int firstPass = (int)Math.Floor(level + (300.0f * Math.Pow(2.0f, level / 7.0f)));
            return firstPass / 4;
        }

        public int TotalExperienceRequiredForALevel(int level)
        {
            int firstPass = 0;
            int secondPass = 0;

            for (int levelCycle = 1; levelCycle < level; levelCycle++)
            {
                firstPass += (int)Math.Floor(levelCycle + (300.0f * Math.Pow(2.0f, levelCycle / 7.0f)));
                secondPass = firstPass / 4;
            }
            return secondPass;
        }

        public int GetXPforLevel(int level, int maxExp, int maxLvl)
        {
            if (level > maxLvl)
                return 0;

            int firstPass = 0;
            int secondPass = 0;
            for (int levelCycle = 1; levelCycle < level; levelCycle++)
            {
                firstPass += (int)Math.Floor(levelCycle + (300.0f * Math.Pow(2.0f, levelCycle / 7.0f)));
                secondPass = firstPass / 4;
            }

            if (secondPass > maxExp && maxExp != 0)
                return maxExp;

            if (secondPass < 0)
                return maxExp;

            return secondPass;
        }

        public int GetLevelForXP(int exp, int maxExp, int maxLvl)
        {
            if (exp > maxExp)
                return maxExp;

            int firstPass = 0;
            for (int levelCycle = 1; levelCycle <= maxLvl; levelCycle++)
            {
                firstPass += (int)Math.Floor(levelCycle + (300.0f * Math.Pow(2.0f, levelCycle / 7.0f)));
                int secondPass = firstPass / 4;
                if (secondPass > exp)
                    return levelCycle;
            }

            return 0;
        }

        /// <summary>
        /// Need to update
        /// </summary>
        /// <param name="currentLevel"></param>
        /// <param name="experience"></param>
        /// <param name="amount"></param>
        /// <param name="maxExp"></param>
        /// <param name="maxLevel"></param>
        /// <returns></returns>
        public bool AddExp(int currentLevel, int experience, int amount, int maxExp, int maxLevel)
        {
            if (amount + experience < 0 || experience > maxExp)
            {
                if (experience > maxExp)
                    experience = maxLevel;

                return false;
            }

            int oldLevel = GetLevelForXP(experience, maxExp, maxLevel);
            experience += amount;

            if (oldLevel < GetLevelForXP(experience, maxExp, maxLevel))
            {
                if (currentLevel < GetLevelForXP(experience, maxExp, maxLevel))
                {
                    int newLevel = GetLevelForXP(experience, maxExp, maxLevel);

                    // if (OnLevelUp != null)
                    //    OnLevelUp.Invoke();

                    return true;
                }
            }

            return false;
        }
    }
}
