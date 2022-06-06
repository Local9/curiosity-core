namespace Curiosity.Framework.Shared
{
    internal class Common
    {
        public static async Task MoveToMainThread() => await BaseScript.Delay(0);
    }
}
