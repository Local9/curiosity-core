using Curiosity.Missions.Client.net.DataClasses;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using System;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Scripts
{
    static class SoundManager
    {
        static Client client = Client.GetInstance();

        static public float AudioVolume = 0.5f;

        static public void PlayAudio(string audioFiles)
        {
            SoundMessage soundMessage = new SoundMessage(AudioVolume);
            foreach (string audioFile in audioFiles.Split(' '))
            {
                soundMessage.audioQueue.Add($"./audio/{audioFile}.wav");
            }
            SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
        }

        static public void PlayAudioFile(string audioFile)
        {
            SoundMessage soundMessage = new SoundMessage(audioFile, AudioVolume);
            SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
        }
    }
}
