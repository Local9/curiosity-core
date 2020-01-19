using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class SessionCollection
    {
        public string Id { get; set; }
        public List<int> Players { get; set; } = new List<int>();
    }
}