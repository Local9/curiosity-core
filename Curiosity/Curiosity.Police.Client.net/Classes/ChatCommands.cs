using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Curiosity.Police.Client.net.Classes
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();

        static float _audioVolume = 0.5f;

        public static void Init()
        {
            API.RegisterCommand("audio", new Action<int, List<object>, string>(OnAudioTest), false);
            API.RegisterCommand("volume", new Action<int, List<object>, string>(OnAudioVolume), false);
        }

        static void OnAudioVolume(int playerHandle, List<object> arguments, string raw)
        {
            if (arguments.Count < 1) return;
            float defaultValue = 0.5f;
            float.TryParse($"{arguments[0]}", out defaultValue);
            _audioVolume = defaultValue;
            CitizenFX.Core.UI.Screen.ShowNotification($"Volume Updated: {_audioVolume}");
        }

        static async void OnAudioTest(int playerHandle, List<object> arguments, string raw)
        {

            int handle = Game.PlayerPed.Handle;

            if (!Game.Player.Character.IsAmbientSpeechEnabled)
                CitizenFX.Core.UI.Screen.ShowNotification($"ENABLE SPEECH");

            Game.PlayerPed.PlayAmbientSpeech("A_F_M_BEACH_01_WHITE_FULL_01", "BUMP_01", SpeechModifier.Standard);

            //PlayAmbientSpeechWithVoice(Game.PlayerPed.Handle, "STOP_THE_CAR", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE", false);

            //Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Game.Player.Character, "CODE_HUMAN_MEDIC_TIME_OF_DEATH", 0, 1);
            //Function.Call(Hash._PLAY_AMBIENT_SPEECH_WITH_VOICE, Game.Player.Character, "GENERIC_INSULT_HIGH", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE_SHOUTED", 0);
            //await Client.Delay(5000);
            //Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, Game.Player.Character);

            //List<SoundMessage> soundMessages = new List<SoundMessage>();
            //soundMessages.Add(new SoundMessage("ATTENTION_ALL_UNITS/ATTENTION_ALL_UNITS_03", _audioVolume, 1080));
            //soundMessages.Add(new SoundMessage("WE_HAVE/WE_HAVE_02", _audioVolume, 600));
            //soundMessages.Add(new SoundMessage("CONJUNCTIVES/A_01", _audioVolume, 2300));
            //soundMessages.Add(new SoundMessage("SUSPECTS/SUSPECTS_HEADING_02", _audioVolume, 1100));
            //soundMessages.Add(new SoundMessage("CONJUNCTIVES/A_02", _audioVolume, 1800));
            //soundMessages.Add(new SoundMessage("DIRECTION/DIRECTION_HEADING_EAST_01", _audioVolume, 600));
            //soundMessages.Add(new SoundMessage("CONJUNCTIVES/ON_02", _audioVolume, 1300));
            //soundMessages.Add(new SoundMessage("STREETS/STREET_GREAT_OCEAN_HWY_01", _audioVolume, 1100));
            //await Client.Delay(10);
            //foreach (SoundMessage soundMessage in soundMessages)
            //{
            //    SendNuiMessage(JsonConvert.SerializeObject(soundMessage));
            //    await Client.Delay(soundMessage.delayAudio);
            //}

        }
    }

    class SoundMessage
    {
        public string transactionType;
        public string transactionFile;
        public float transactionVolume;
        public int delayAudio;

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
