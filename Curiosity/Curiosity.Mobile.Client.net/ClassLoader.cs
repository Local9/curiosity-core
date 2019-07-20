using Curiosity.Shared.Client.net;

namespace Curiosity.Mobile.Client.net
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

            // Mobile Phone Addons
            Mobile.MobilePhone.Init();
            Mobile.Apps.PlayerListApplication.Init();
            Mobile.Apps.JobApplication.Init();
            Mobile.Apps.SettingsApplication.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }
    }
}