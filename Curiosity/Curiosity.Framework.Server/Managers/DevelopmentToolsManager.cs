using CitizenFX.Core.Native;
using Curiosity.Framework.Server.Events;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using System.IO;
using System.Text.RegularExpressions;

namespace Curiosity.Framework.Server.Managers
{
    public class DevelopmentToolsManager : Manager<DevelopmentToolsManager>
    {
        public override void Begin()
        {
            EventDispatcher.Mount("developer:position:camera:save", new Func<ClientId, Character, CameraInformation, Task<bool>>(OnSaveCameraPositionAsync));
        }

        private async Task<bool> OnSaveCameraPositionAsync(ClientId client, Character character, CameraInformation camera)
        {
            // TODO: Add developer check
            try
            {
                string name = Regex.Replace(client.Player.Name, "[^a-zA-Z0-9]", String.Empty);

                string filePath = $@"{API.GetResourcePath(API.GetCurrentResourceName())}\dev-data\{name}-CameraPositions.txt";
                EvaluatePath(filePath);

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }

                string newLine = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}, {camera}";
                Logger.Info($"Saving Camera Position: {newLine}");

                using (TextWriter tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine(newLine);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occured when saving camera position");
                Logger.Error($"{ex}");
                return false;
            }
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
