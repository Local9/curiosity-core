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
        }


        static async void OnAudioTest(int playerHandle, List<object> arguments, string raw)
        {
            int handle = Game.PlayerPed.Handle;

            if (!Game.Player.Character.IsAmbientSpeechEnabled)
                CitizenFX.Core.UI.Screen.ShowNotification($"IsAmbientSpeechEnabled");

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
}
