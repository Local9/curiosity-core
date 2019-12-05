using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions
{
    class Social
    {
        static public async void Hello(Ped ped)
        {
            await Helpers.LoadAnimation("gestures@m@sitting@generic@casual");

            if (Client.speechType == SpeechType.NORMAL)
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (ped.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                List<string> hello = new List<string>() { "GENERIC_HI", "KIFFLOM_GREET" };
                PlayAmbientSpeechWithVoice(ped.Handle, hello[Client.Random.Next(hello.Count)], voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_hello", 8.0f, -1, (AnimationFlags)49);
            }
            else
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (ped.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                PlayAmbientSpeechWithVoice(ped.Handle, "GENERIC_INSULT_HIGH", voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_what_hard", 8.0f, -1, (AnimationFlags)49);
            }
            await Client.Delay(1000);
            Game.PlayerPed.Task.ClearAll();
        }
    }
}
