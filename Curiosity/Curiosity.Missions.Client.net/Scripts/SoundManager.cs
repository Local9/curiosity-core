using Curiosity.Missions.Client.DataClasses;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts
{
    static class SoundManager
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        static public float AudioVolume = 0.5f;

        static public void PlayAudio(string audioFiles)
        {
            SoundMessage soundMessage = new SoundMessage(AudioVolume);
            foreach (string audioFile in audioFiles.Split(' '))
            {
                string file = $"./audio/{audioFile}.wav";
                soundMessage.audioQueue.Add(file);
            }
            string message = JsonConvert.SerializeObject(soundMessage);
            SendNuiMessage(message);
        }
        static public void PlaySFX(string audioFiles)
        {
            SoundMessage soundMessage = new SoundMessage(AudioVolume);
            foreach (string audioFile in audioFiles.Split(' '))
            {
                string file = $"./sfx/{audioFile}.wav";
                soundMessage.audioQueue.Add(file);
            }
            string message = JsonConvert.SerializeObject(soundMessage);
            SendNuiMessage(message);
        }
        static public void PlaySFX(string audioFiles, float audioVolume)
        {
            if (audioVolume > AudioVolume)
                audioVolume = AudioVolume;

            SoundMessage soundMessage = new SoundMessage(audioVolume);
            foreach (string audioFile in audioFiles.Split(' '))
            {
                string file = $"./sfx/{audioFile}.wav";
                soundMessage.audioQueue.Add(file);
            }
            string message = JsonConvert.SerializeObject(soundMessage);
            SendNuiMessage(message);
        }

        static public void PlayAudioFile(string audioFile)
        {
            SoundMessage soundMessage = new SoundMessage(audioFile, AudioVolume);
            SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
        }
    }
}
