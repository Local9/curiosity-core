using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Callouts.Client.Managers
{
    class PlayerManager : BaseScript
    {
        static ExportDictionary exportDictionary;

        public PlayerManager()
        {

        }

        public static bool IsDeveloper()
        {
            return false; // NEED TO FIX NEWTONSOFT FUCK BOY ERRORS
        }
    }
}
