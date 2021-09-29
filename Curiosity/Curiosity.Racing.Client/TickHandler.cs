using System;

namespace Curiosity.Racing.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}