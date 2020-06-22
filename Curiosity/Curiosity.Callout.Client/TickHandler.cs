using System;

namespace Curiosity.Callout.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}