using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Interface;
using Curiosity.Systems.Client.Interface.Modules;
using Curiosity.Systems.Client.Managers;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment.Entities.Models
{
    public class VoiceChat : Manager<VoiceChat>
    {
        public int Channel { get; set; }
        public float Range { get; set; }

        public void Commit()
        {
            if (Channel != 0 && Channel != 1)
            {
                Function.Call(Hash.NETWORK_SET_VOICE_CHANNEL, Channel);
            }
            else
            {
                Function.Call(Hash.NETWORK_CLEAR_VOICE_CHANNEL);
            }


            Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, Range);
        }

        public void IterateRange()
        {
            switch (Range)
            {
                case VoiceChatRange.Whisper:
                    Range = VoiceChatRange.Normal;

                    break;
                case VoiceChatRange.Normal:
                    Range = VoiceChatRange.Shout;

                    break;
                case VoiceChatRange.Shout:
                    Range = VoiceChatRange.Whisper;

                    break;
                default:
                    Range = VoiceChatRange.Normal;

                    break;
            }
        }

        public string Translate()
        {
            switch (Range)
            {
                case VoiceChatRange.Whisper:
                    return "Whisper";
                case VoiceChatRange.Normal:
                    return "Normal";
                case VoiceChatRange.Shout:
                    return "Shout";
                default:
                    return ".";
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnVoiceChatTick()
        {
            var hud = HeadupDisplay.GetModule();

            if (!hud.IsDisabled)
            {
                var ped = API.PlayerPedId();
                var anchor = hud.GetMinimapAnchor();

                ScreenInterface.DrawText(
                    $"Voice: {(API.NetworkIsPlayerTalking(API.PlayerId()) ? "~b~" : "")}{Translate()}", 0.25f,
                    !API.IsPedSittingInAnyVehicle(ped)
                        ? new Vector2(anchor.X + 0.0005f,
                            (float)(anchor.BottomY - anchor.UnitY * 18f / 2 / 2) - anchor.UnitY * 18f * 2 - 0.0005f)
                        : new Vector2(anchor.X + anchor.Width + 0.001f, (float)(anchor.BottomY - anchor.Height / 2)),
                    Color.FromArgb(200, 200, 200, 200));

                if (Game.IsControlPressed(0, Control.Sprint) && Game.IsControlJustPressed(0, Control.VehicleHeadlight))
                {
                    IterateRange();
                }
            }

            await Task.FromResult(0);
        }
    }

    public static class VoiceChatRange
    {
        public const float Whisper = 1f;
        public const float Normal = 5f;
        public const float Shout = 12f;
    }
}