using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Client.Extensions
{
    public static class UserExtensions
    {
        public static void PostUpdates(this CuriosityUser user)
        {
            EventSystem.GetModule().Send("user:postupdates", user);
        }
        public static void SendEvent(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, payloads);
        }

        public static void Send(this CuriosityUser user, string target)
        {
            Send(user, target, null);
        }

        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            if (payloads == null) payloads = new object[] { false };

            var modified = new object[2 + payloads.Length];
            var index = 2;

            modified[0] = user.Handle;
            modified[1] = target;

            foreach (var payload in payloads)
            {
                modified[index] = payload;

                index++;
            }

            EventSystem.GetModule().Send("user:redirect", modified);
        }
    }
}