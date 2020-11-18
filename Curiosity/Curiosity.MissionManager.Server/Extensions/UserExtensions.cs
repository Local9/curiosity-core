using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.MissionManager.Server.Extensions
{
    static class UserExtensions
    {
        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }
    }
}
