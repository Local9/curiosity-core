using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Extensions
{
    static class UserExtensions
    {
        public static void Send(this CuriosityUser user, string target, params object[] payloads)
        {
            EventSystem.GetModule().Send(target, user.Handle, payloads);
        }
    }
}
