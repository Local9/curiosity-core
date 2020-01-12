using System;

namespace Curiosity.System.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}