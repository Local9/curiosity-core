using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Environment.Entities.Models
{
    class SoundMessage
    {
        public string transactionType;
        public string transactionFile;
        public float transactionVolume;
        public int delayAudio;

        public List<string> audioQueue = new List<string>();

        public SoundMessage(float vol = 0.3f)
        {
            transactionType = "playSound";
            transactionVolume = vol;
        }

        public SoundMessage(string file, float vol = 0.3f, int delay = 1000)
        {
            transactionFile = file;
            transactionType = "playSound";
            transactionVolume = vol;
            delayAudio = delay;
        }

        public SoundMessage(string type, string file, float vol = 0.3f)
        {
            transactionFile = file;
            transactionType = type;
            transactionVolume = vol;
        }
    }
}
