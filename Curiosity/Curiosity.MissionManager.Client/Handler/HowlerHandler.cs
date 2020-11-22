using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Handler
{
    class HowlerHandler
    {
        public static float AudioVolume = 0.3f;

        public static void PlayAudio(List<string> audioFiles)
        {
            SoundMessage soundMessage = new SoundMessage(AudioVolume);

            audioFiles.ForEach(file =>
            {
                string fileLoc = $"./audio/{file}.wav";
                soundMessage.audioQueue.Add(fileLoc);
            });

            string message = JsonConvert.SerializeObject(soundMessage);
            API.SendNuiMessage(message);
        }

        public static void PlaySFX(List<string> audioFiles)
        {
            SoundMessage soundMessage = new SoundMessage(AudioVolume);

            audioFiles.ForEach(file =>
            {
                string fileLoc = $"./sfx/{file}.wav";
                soundMessage.audioQueue.Add(fileLoc);
            });

            string message = JsonConvert.SerializeObject(soundMessage);
            API.SendNuiMessage(message);
        }

        public static void PlaySFX(List<string> audioFiles, float audioVolume)
        {
            if (audioVolume > AudioVolume)
                audioVolume = AudioVolume;

            SoundMessage soundMessage = new SoundMessage(audioVolume);

            audioFiles.ForEach(file =>
            {
                string fileLoc = $"./sfx/{file}.wav";
                soundMessage.audioQueue.Add(fileLoc);
            });

            string message = JsonConvert.SerializeObject(soundMessage);
            API.SendNuiMessage(message);
        }

        public static void PlayAudioFile(string audioFile)
        {
            SoundMessage soundMessage = new SoundMessage(audioFile, AudioVolume);
            API.SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
        }
    }
}
