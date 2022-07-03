namespace Curiosity.Framework.Client.Engine
{
    internal class SoundEngine
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
            StartAudioScene(audioScene);
        }

        public void PlayFrontend(string audioName, string audioRef)
        {
            PlaySoundFrontend(-1, audioName, audioRef, false);
        }

        public void Stop(string audioScene)
        {
            StopAudioScene(audioScene);
        }
    }
}
