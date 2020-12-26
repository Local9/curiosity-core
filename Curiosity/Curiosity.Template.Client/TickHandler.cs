using System;

namespace Curiosity.Template.Client
{
    public class TickHandler : Attribute
    {
        public bool SessionWait { get; set; }
    }
}