using System;

namespace Curiosity.Systems.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}