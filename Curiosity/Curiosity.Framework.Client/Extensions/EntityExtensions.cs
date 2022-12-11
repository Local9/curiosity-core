namespace Curiosity.Framework.Client.Extensions
{
    internal static class EntityExtensions
    {
        public async static Task FadeEntityAsync(this Entity entity, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
                Function.Call(Hash.NETWORK_FADE_IN_ENTITY, entity.Handle, fadeOutNormal, slow);
            else
                NetworkFadeOutEntity(entity.Handle, fadeOutNormal, slow);

            while (NetworkIsEntityFading(entity.Handle)) await BaseScript.Delay(0);
        }
        public static void FadeEntity(this Entity entity, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
                Function.Call(Hash.NETWORK_FADE_IN_ENTITY, entity.Handle, fadeOutNormal, slow);
            else
                NetworkFadeOutEntity(entity.Handle, fadeOutNormal, slow);
        }
    }
}
