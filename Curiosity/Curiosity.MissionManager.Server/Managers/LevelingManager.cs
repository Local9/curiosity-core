using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Server.Managers
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
	}
}
