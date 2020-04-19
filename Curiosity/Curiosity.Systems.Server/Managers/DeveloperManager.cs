using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using System;
using System.IO;

namespace Curiosity.Systems.Server.Managers
{
    public class DeveloperManager : Manager<DeveloperManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("developer:savePos", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[metadata.Sender];

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
                    
                    Player player = CuriosityPlugin.PlayersList[metadata.Sender];

                    Vector3 pos = player.Character.Position;
                    float x = pos.X;
                    float y = pos.Y;
                    float z = pos.Z;
                    float h = player.Character.Heading;

                    Logger.Debug($"Saving Position: {posName} - {pos} : {h}");

                    tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{posName},new Vector3({x}f, {y}f, {z}f),({x}, {y}, {z}),(x: {x}, y: {y}, z: {z}),{h}");
                }

                return true;
            }));

        }
    }
}
