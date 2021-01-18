using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Curiosity.Core.Server.Managers
{
    public class DeveloperManager : Manager<DeveloperManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("developer:savePos", new EventCallback(metadata =>
                {
                    try
                    {
                        CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                        if (!curiosityUser.IsDeveloper) return null;

                        string name = Regex.Replace(curiosityUser.LatestName, "[^a-zA-Z0-9]", String.Empty);

                        string filePath = $@"{API.GetResourcePath(API.GetCurrentResourceName())}\data\{name}-savedPositions.txt";

                        EvaluatePath(filePath);

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

                            Logger.Debug($"Saving Position: {posName} (\"X\": {x}, \"Y\": {y}, \"Z\": {z}) : {h}");

                            tw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")},{posName},new Vector3({x}f, {y}f, {z}f),({x}, {y}, {z}),(X: {x}, Y: {y}, Z: {z}),{h}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"savePos -> {ex.Message}");
                    }

                    return true;
                }));
        }

        private String EvaluatePath(String path)
        {

            try
            {
                String folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder))
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(folder);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
                return "";
            }
            return path;
        }
    }
}
