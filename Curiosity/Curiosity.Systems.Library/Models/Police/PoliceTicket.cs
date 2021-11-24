using Curiosity.Systems.Library.Enums;
using System;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Police
{
    [DataContract]
    public class PoliceTicket
    {
        [DataMember(Name = "id")]
        public int Id;

        [DataMember(Name = "policeTicketTypeId")]
        public ePoliceTicketType PoliceTicketType;

        [DataMember(Name = "characterId")]
        public int CharacterId;

        [DataMember(Name = "characterName")]
        public string CharacterName;

        [DataMember(Name = "ticketDate")]
        public DateTime TicketDate;

        [DataMember(Name = "ticketPaymentDue")]
        public DateTime TicketPaymentDue;

        [DataMember(Name = "ticketPaid")]
        public DateTime? TicketPaid;

        [DataMember(Name = "ticketValue")]
        public long TicketValue;
    }
}
