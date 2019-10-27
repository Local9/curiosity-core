using Curiosity.Missions.Client.net.DataClasses;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts
{
    static class SoundManager
    {
        static Client client = Client.GetInstance();

        static public float AudioVolume = 0.5f;

        static public void PlayAudioFile(string audioFile)
        {
            SoundMessage soundMessage = new SoundMessage(audioFile, AudioVolume);
            SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
        }
    }
}
