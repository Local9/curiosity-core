using CitizenFX.Core.Native;

namespace Curiosity.Systems.Client.Environment.Entities.Models
{
    public class SoundSystem
    {
        public bool IsDisabled { get; protected set; }

        public void Disable()
        {
            IsDisabled = true;

            Play("MP_LEADERBOARD_SCENE");
        }

        public void Enable()
        {
            IsDisabled = false;

            Stop("MP_LEADERBOARD_SCENE");
        }

        public void Play(string audioScene)
        {
            API.StartAudioScene(audioScene);
        }

        public void PlayFrontend(string audioName, string audioRef)
        {
            API.PlaySoundFrontend(-1, audioName, audioRef, false);
        }

        public void Stop(string audioScene)
        {
            API.StopAudioScene(audioScene);
        }
    }
}