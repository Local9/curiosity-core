using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Models
{
    public class Party
    {
        public int PartyId { get; internal set; }
        public List<string> Members { get; internal set; }

        public Party(int partyId)
        {
            PartyId = partyId;
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
