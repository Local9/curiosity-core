using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] public CuriosityUser User { get; set; }
        [JsonIgnore] public CuriosityEntity Entity { get; set; }
        [JsonIgnore] public CuriosityCharacter Character { get; set; }
        public ulong DiscordId { get; set; }
        public int Handle { get; set; }
        public string Name { get; set; }
        public DateTime Joined { get { return User.DateCreated; } }
        public Role Role { get { return User.Role; } }
        public string RoleDescription { get { return $"{User.Role}"; } }

        public CuriosityPlayer(ulong discordId, CuriosityEntity entity)
        {
            DiscordId = discordId;
            Entity = entity;
        }
    }
}
