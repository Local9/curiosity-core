using System;

namespace Curiosity.Interface.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}