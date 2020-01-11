using System;

namespace Atlas.Roleplay.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}