using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Extensions
{
    static class ScriptExtended
    {
        public static void TerminateScriptByName(string name)
        {
            if (Function.Call<bool>(unchecked((Hash)(-286976521679683174L)), new InputArgument[] { name }))
            {
                Function.Call(unchecked((Hash)(-7077668788463384353L)), new InputArgument[] { name });
            }
        }
    }
}
