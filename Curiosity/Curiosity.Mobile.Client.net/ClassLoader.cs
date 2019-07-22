using Curiosity.Shared.Client.net;
using System;

namespace Curiosity.Mobile.Client.net
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            // Mobile Phone Addons
            Mobile.MobilePhone.Init();
            Mobile.Apps.PlayerListApplication.Init();
            Mobile.Apps.JobApplication.Init();
            Mobile.Apps.SkillsApplication.Init();
            Mobile.Apps.SettingsApplication.Init();

            client.RegisterEventHandler("curiosity:Mobile:Job:Active", new Action<bool>(OnJobActive));

            Log.Verbose("Leaving ClassLoader Init");
        }

        static void OnJobActive(bool active)
        {
            Mobile.MobilePhone.IsJobActive = active;
        }
    }
}