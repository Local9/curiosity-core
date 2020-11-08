using Curiosity.Systems.Library.Models;
using Curiosity.MissionManager.Server.Events;

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
