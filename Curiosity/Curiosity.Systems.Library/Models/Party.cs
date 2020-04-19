using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class Party
    {
        public Guid PartyId { get; internal set; }
        public int LeaderHandle { get; set; }
        public List<int> Members { get; internal set; }

        public void AddMember(int member)
        {
            Members.Add(member);
        }

        public void RemoveMember(int member)
        {
            Members.Remove(member);
        }
    }
}
