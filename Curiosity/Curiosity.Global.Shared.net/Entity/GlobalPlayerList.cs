using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Global.Shared.net.Entity
{
    public class GlobalPlayerList
    {
        public int UserId;
        public int SessionId;
        public string PlayerName;
        public bool IsDonator;
        public bool IsStaff;
        public int Ping;
        // Custom Addons
        public string CurrentJob;
    }
}
