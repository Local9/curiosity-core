using CitizenFX.Core;
using static Curiosity.Shared.Client.net.Helper.NativeWrappers;

namespace Curiosity.Shared.Client.net.Classes.Environment
{
    public static class Experience
    {
        public static void Display(Vector3 dmgPos, string skill, int xp, int timeout, bool increase)
        {
            string message = $"{xp}xp";
            if (increase)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Skills:Increase", skill, xp);
            }
            else
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Skills:Decrease", skill, xp);
                message = $"-{xp}xp";
            }
            Draw3DTextTimeout(dmgPos.X, dmgPos.Y, dmgPos.Z, message, timeout, 40f, 60.0f);
        }
    }
}
