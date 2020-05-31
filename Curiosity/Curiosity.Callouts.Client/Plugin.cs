using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Curiosity.Callouts.Client
{
    public class Plugin : BaseScript
    {
        private static string resourceName = API.GetCurrentResourceName();
        private string resourcePath = $"resources/{resourceName}/callouts/";

        public Plugin()
        {
            DirectoryInfo dir = new DirectoryInfo(resourcePath);

            foreach (FileInfo file in dir.GetFiles("*.net.dll"))
            {

            }
        }
    }
}
