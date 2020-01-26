using System;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityUser
    {
        public int Handle { get; set; }
        public ulong DiscordId { get; set; }
        public string License { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public DateTime LatestActivity { get; set; }

        public CuriosityCharacter Character { get; set; }
    }
}