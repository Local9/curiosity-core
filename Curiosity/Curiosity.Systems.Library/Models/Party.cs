using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Systems.Library.Models
{
    public class Party
    {
        public Guid PartyId { get; internal set; }
        public string LeaderHandle { get; set; }
        public List<PartyMember> Members { get; internal set; } = new List<PartyMember>();

        public Party()
        {
            this.PartyId = Guid.NewGuid();
        }

        public void AddMember(string memberHandle, string name)
        {
            PartyMember partyMember = new PartyMember();
            partyMember.Handle = memberHandle;
            partyMember.Name = name;
            Members.Add(partyMember);
        }

        public void RemoveMember(string memberHandle)
        {
            PartyMember partyMember = Members.Select(p => p).Where(x => x.Handle == memberHandle).FirstOrDefault();

            if (partyMember != null)
                Members.Remove(partyMember);
        }
    }
}
