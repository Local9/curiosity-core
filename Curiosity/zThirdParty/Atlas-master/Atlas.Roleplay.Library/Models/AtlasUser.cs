using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Roleplay.Library.Models
{
    public class AtlasUser
    {
        [Key] public string Seed { get; set; }
        public int Handle { get; set; }
        public string SteamId { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public DateTime LatestActivity { get; set; }
        public List<Tuple<string, DateTime>> ConnectionHistory { get; set; }
        public UserMetadata Metadata { get; set; }
        public List<AtlasCharacter> Characters { get; set; } = new List<AtlasCharacter>();

        [Column("Metadata")]
        public string _Metadata
        {
            get => JsonConvert.SerializeObject(Metadata);
            set => Metadata = JsonConvert.DeserializeObject<UserMetadata>(value);
        }

        [Column("ConnectionHistory")]
        public string _ConnectionHistory
        {
            get => JsonConvert.SerializeObject(ConnectionHistory);
            set => ConnectionHistory = JsonConvert.DeserializeObject<List<Tuple<string, DateTime>>>(value);
        }
    }

    public class UserMetadata
    {
    }
}