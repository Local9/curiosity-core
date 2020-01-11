using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Server.Events;

namespace Atlas.Roleplay.Server.Extensions
{
    public static class UserExtensions
    {
        public static void Send(this AtlasUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }
    }
}