using Curiosity.Systems.Library.Models;
using Curiosity.Core.Server.Events;

namespace Curiosity.Core.Server.Extensions
{
    static class UserExtensions
    {
        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }
    }
}
