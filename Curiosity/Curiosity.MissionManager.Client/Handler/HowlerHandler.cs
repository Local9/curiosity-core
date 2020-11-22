using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Handler
{
    class HowlerHandler
    {
        public static float AudioVolume = 0.3f;
        static bool audioAlreadyPlaying = false;

        public static async void PlayAudio(List<string> audioFiles)
        {
            if (audioAlreadyPlaying) return;
            audioAlreadyPlaying = true;

            SoundMessage soundMessage = new SoundMessage(AudioVolume);

            audioFiles.ForEach(file =>
            {
                string fileLoc = $"./audio/{file}.wav";
                soundMessage.audioQueue.Add(fileLoc);
            });

            string message = JsonConvert.SerializeObject(soundMessage);
            API.SendNuiMessage(message);

            await BaseScript.Delay(10000);
            audioAlreadyPlaying = false;
        }

        public static async void PlaySFX(List<string> audioFiles)
        {
            if (audioAlreadyPlaying) return;
            audioAlreadyPlaying = true;

            SoundMessage soundMessage = new SoundMessage(AudioVolume);

            audioFiles.ForEach(file =>
            {
                string fileLoc = $"./sfx/{file}.wav";
                soundMessage.audioQueue.Add(fileLoc);
            });

            string message = JsonConvert.SerializeObject(soundMessage);
            API.SendNuiMessage(message);

            await BaseScript.Delay(10000);
            audioAlreadyPlaying = false;
        }

        public static async void PlaySFX(List<string> audioFiles, float audioVolume)
        {
            if (audioAlreadyPlaying) return;
            audioAlreadyPlaying = true;

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

            await BaseScript.Delay(10000);
            audioAlreadyPlaying = false;
        }

        public static async void PlayAudioFile(string audioFile)
        {
            if (audioAlreadyPlaying) return;
            audioAlreadyPlaying = true;

            SoundMessage soundMessage = new SoundMessage(audioFile, AudioVolume);
            API.SendNuiMessage(JsonConvert.SerializeObject(soundMessage));

            await BaseScript.Delay(10000);
            audioAlreadyPlaying = false;
        }
    }
}
