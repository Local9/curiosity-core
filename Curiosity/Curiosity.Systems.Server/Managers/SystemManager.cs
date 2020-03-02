using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using Curiosity.Systems.Server.MySQL;
using System;
using System.IO;

namespace Curiosity.Systems.Server.Managers
{
    public class SystemManager : Manager<SystemManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["onResourceStop"] += new Action<string>(OnResourceStop);

            EventSystem.GetModule().Attach("system:savePos", new AsyncEventCallback(async metadata =>
            {
                Player player = CuriosityPlugin.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[player.Handle];

                if (!curiosityUser.IsDeveloper) return null;

                string filePath = $@"{API.GetResourcePath(API.GetCurrentResourceName())}\data\{curiosityUser.LastName}-savedPositions.txt";

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();

                    using (TextWriter tw = new StreamWriter(filePath))
                    {
                        tw.WriteLine("Date,Name,VectorCSharp,VectorLua,VectorJson,Heading");
                    }
                }

                using (TextWriter tw = new StreamWriter(filePath, true))
                {
                    string posName = metadata.Find<string>(0);
                    float x = metadata.Find<float>(1);
                    float y = metadata.Find<float>(2);
                    float z = metadata.Find<float>(3);
                    float h = metadata.Find<float>(4);
                    
                    Logger.Debug($"Saving Position: {posName}");

                    tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{posName},new Vector3({x}f, {y}f, {z}f),({x}, {y}, {z}),(x: {x}, y: {y}, z: {z}),{h}");
                }

                return true;
            }));

        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != API.GetCurrentResourceName()) return;

            Logger.Info($"Stopping Curiosity Systems");
        }
    }
}
