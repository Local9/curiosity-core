using System.Collections.Generic;

namespace Curiosity.Missions.Client.DataClasses
{
    class SoundMessage
    {
        public string transactionType;
        public string transactionFile;
        public float transactionVolume;
        public int delayAudio;

        public List<string> audioQueue = new List<string>();

        public SoundMessage(float vol)
        {
            transactionType = "playSound";
            transactionVolume = vol;
        }

        public SoundMessage(string file, float vol, int delay = 1000)
        {
            transactionFile = file;
            transactionType = "playSound";
            transactionVolume = vol;
            delayAudio = delay;
        }

        public SoundMessage(string type, string file, float vol)
        {
            transactionFile = file;
            transactionType = type;
            transactionVolume = vol;
        }
    }
}
