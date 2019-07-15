using Curiosity.Shared.Client.net;
using CitizenFX.Core;

namespace Curiosity.Interface.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            Environment.UI.PlayerHeadshot.Init();
            Environment.UI.DutyIcons.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}