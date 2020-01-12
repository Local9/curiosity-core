using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Curiosity.System.Library.Inventory;
//using Curiosity.System.Library.LawEnforcement;
using Newtonsoft.Json;

namespace Curiosity.System.Library.Models
{
    public class CuriosityCharacter
    {
        [Key] public string Seed { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Fullname => $"{Name} {Surname}";
        public string DateOfBirth { get; set; }
        public int LastDigits { get; set; }
        public int Health { get; set; }
        public int Shield { get; set; }
        public long Cash { get; set; }
        public Style Style { get; set; }
        public BankAccount BankAccount { get; set; }
        public bool MarkedAsRegistered { get; set; }
        public CharacterMetadata Metadata { get; set; }

        [Column("Style")]
        public string _Style
        {
            get => JsonConvert.SerializeObject(Style);
            set => Style = JsonConvert.DeserializeObject<Style>(value);
        }

        [Column("BankAccount")]
        public string _BankAccount
        {
            get => JsonConvert.SerializeObject(BankAccount);
            set => BankAccount = JsonConvert.DeserializeObject<BankAccount>(value);
        }

        [Column("Metadata")]
        public string _Metadata
        {
            get => JsonConvert.SerializeObject(Metadata);
            set => Metadata = JsonConvert.DeserializeObject<CharacterMetadata>(value);
        }
    }

    public class CharacterMetadata
    {
        public string Employment { get; set; }
        public int EmploymentRole { get; set; }
        public List<InventoryContainerBase> Inventories { get; set; }
        public Position LastPosition { get; set; }
        public Tuple<int, int> EquippedMask { get; set; }
        public Dictionary<string, Style> SavedOutfits { get; set; }
        //public List<JailCase> JailCases { get; set; }
        //[JsonIgnore] public JailCase ActiveJailCase => JailCases?.FirstOrDefault(self => self.IsActive);
    }

    public class BankAccount
    {
        public long Balance { get; set; }
        public List<BankTransaction> History { get; set; }
    }

    public class BankTransaction
    {
        public BankTransactionType Type { get; set; }
        public long Amount { get; set; }
        [JsonIgnore] public DateTime Date { get; set; }
        public string Information { get; set; }

        [JsonProperty("Date")] public string _Date => Date.ToString("MM/dd/yyyy HH:mm:ss");
    }

    public enum BankTransactionType
    {
        Withdraw,
        Deposit
    }
}