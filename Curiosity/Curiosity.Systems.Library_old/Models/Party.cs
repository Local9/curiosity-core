using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Systems.Library.Models
{
    public class Party
    {
        public Guid PartyId { get; internal set; }
        public int LeaderHandle { get; set; }
        public List<PartyMember> Members { get; internal set; } = new List<PartyMember>();
        public string NewestMember { get; internal set; }

        public Party()
        {
            this.PartyId = Guid.NewGuid();
        }

        public void AddMember(int memberHandle, string name)
        {
            PartyMember partyMember = new PartyMember();
            partyMember.Handle = memberHandle;
            NewestMember = name;
            partyMember.Name = name;
            Members.Add(partyMember);
        }

        public void RemoveMember(int memberHandle)
        {
            PartyMember partyMember = Members.Select(p => p).Where(x => x.Handle == memberHandle).FirstOrDefault();

            if (partyMember != null)
                Members.Remove(partyMember);
        }
    }
}
