using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Roleplay.Library.Billing
{
    public class Bill
    {
        [Key] public string Seed { get; set; }
        public BillReceiver Receiver { get; set; }
        public BillSender Sender { get; set; }
        public string[] Designation { get; set; }
        public string[] AmountLines { get; set; }
        public int Amount { get; set; }
        public bool IsCreated { get; set; }
        public bool IsActive { get; set; }
        public int BillNumber { get; set; }
        public int ClientNumber { get; set; }
        public int OrderNumber { get; set; }

        [Column("Receiver")]
        public string _Receiver
        {
            get => JsonConvert.SerializeObject(Receiver);
            set => Receiver = JsonConvert.DeserializeObject<BillReceiver>(value);
        }

        [Column("Sender")]
        public string _Sender
        {
            get => JsonConvert.SerializeObject(Sender);
            set => Sender = JsonConvert.DeserializeObject<BillSender>(value);
        }

        [Column("Designation")]
        public string _Designation
        {
            get => JsonConvert.SerializeObject(Designation);
            set => Designation = JsonConvert.DeserializeObject<string[]>(value);
        }

        [Column("AmountLines")]
        public string _AmountLines
        {
            get => JsonConvert.SerializeObject(AmountLines);
            set => AmountLines = JsonConvert.DeserializeObject<string[]>(value);
        }

        public Bill()
        {
            Seed = Library.Seed.Generate();
            Designation = new string[0];
            AmountLines = new string[0];
        }
    }
}