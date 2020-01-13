using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Curiosity.System.Library.Models
{
    public class CuriosityUser
    {
        [Key] public long UserId { get; set; }
        public int Handle { get; set; }
        public ulong DiscordId { get; set; }
        public string License { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public DateTime LatestActivity { get; set; }
        public List<Tuple<string, DateTime>> ConnectionHistory { get; set; }
        public UserMetadata Metadata { get; set; }
        public List<CuriosityCharacter> Characters { get; set; } = new List<CuriosityCharacter>();

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

        public void Send(string v1, object[] v2)
        {
            throw new NotImplementedException();
        }
    }

    public class UserMetadata
    {
    }
}