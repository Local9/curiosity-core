using System;

namespace Curiosity.Police.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}