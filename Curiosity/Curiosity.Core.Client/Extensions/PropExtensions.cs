namespace Curiosity.Core.Client.Extensions
{
    public static class PropExtensions
    {
        public static async Task<int> TurnOnTV(this Prop tv, string tvscreen, int channel, float vol)
        {
            int num = 0;
            AttachTvAudioToEntity(tv.Handle);

            if (!IsNamedRendertargetRegistered(tvscreen))
                RegisterNamedRendertarget(tvscreen, false);

            if (!IsNamedRendertargetLinked((uint)tv.Model.GetHashCode()))
            {
                LinkNamedRendertarget((uint)tv.Model.GetHashCode());
                await BaseScript.Delay(0);
                num = GetNamedRendertargetRenderId(tvscreen);
            }

            SetTvChannel(channel);
            SetTvVolume(vol);
            EnableMovieSubtitles(false);

            return num;
        }

        public static bool IsPropInList(this Prop prop, List<Model> models) => models != null && !Equals(prop, null) && models.Contains(prop.Model);
    }
}
