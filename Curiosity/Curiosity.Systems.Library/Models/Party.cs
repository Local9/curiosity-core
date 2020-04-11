using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class Party
    {
        public Guid PartyId { get; internal set; }
        public string LeaderHandle { get; internal set; }
        public List<string> Members { get; internal set; }

        public Party(string leaderHandle)
        {
            LeaderHandle = leaderHandle;
            AddMember(leaderHandle);
            PartyId = Guid.NewGuid();
        }

        public void AddMember(string member)
        {
            Members.Add(member);
        }

        public void RemoveMember(string member)
        {
            Members.Remove(member);
        }
    }
}
