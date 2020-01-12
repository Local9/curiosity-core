using Curiosity.System.Library.Models;
using Curiosity.System.Server.Events;

namespace Curiosity.System.Server.Extensions
{
    public static class UserExtensions
    {
        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }
    }
}