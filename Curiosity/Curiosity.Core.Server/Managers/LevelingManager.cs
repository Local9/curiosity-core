using System;

namespace Curiosity.Core.Server.Managers
{
    class LevelingManager : Manager<LevelingManager>
    {
		// get level

		// check required
		static int ExperienceToNextLevel(int level)
		{
			int levelMinus = level - 1;
			int firstPass = (int)Math.Floor(level + (300.0f * Math.Pow(2.0f, level / 7.0f)));
			return firstPass / 4;
		}

		static int TotalExperienceRequiredForALevel(int level)
        {
			int firstPass = 0;
			int secondPass = 0;

			for(int levelCycle = 1; levelCycle < level; levelCycle++)
            {
				firstPass += (int)Math.Floor(levelCycle + (300.0f * Math.Pow(2.0f, levelCycle / 7.0f)));
				secondPass = firstPass / 4;
			}
			return secondPass;
        }
	}
}
