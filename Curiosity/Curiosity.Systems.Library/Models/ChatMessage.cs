using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Models
{
    public struct ChatMessage
    {
        public string Role;
        public string ActiveJob;
        public string Channel;
        public string Name;
        public string Message;
    }

    public struct ChatState
    {
        public bool showChat;
    }
}
