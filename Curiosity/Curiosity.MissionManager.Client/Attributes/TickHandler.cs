using System;

namespace Curiosity.MissionManager.Client.Attributes
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}