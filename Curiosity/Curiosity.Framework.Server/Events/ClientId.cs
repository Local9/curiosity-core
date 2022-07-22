using Curiosity.Framework.Shared.Models;
using Lusive.Events;
using Lusive.Events.Attributes;
using Lusive.Snowflake;
using System.Linq;

namespace Curiosity.Framework.Server.Events
{
    [Serialization]
    public partial class ClientId : ISource
    {
        public SnowflakeId Id { get; set; }
        public int Handle { get; set; }
        public User User { get; set; }
        public Identifiers Identifiers => User.Identifiers;

        [Ignore]
        [JsonIgnore]
        public Player Player { get => PluginManager.PlayerList[Handle]; }

        [Ignore]
        [JsonIgnore]
        public Ped Ped { get => Player.Character; }

        public static readonly ClientId Global = new(-1);

        [Ignore]
        public Status Status { get; set; }
        public ClientId()
        {
            Status = new(Player);
        }

        public ClientId(Snowflake id)
        {
            Player owner = PluginManager.PlayerList.FirstOrDefault(x => x.Handle == Handle.ToString());
            if (owner != null)
            {
                Id = id;
                Handle = Convert.ToInt32(owner.Handle);
                LoadUser();
                Status = new(Player);
            }
            else
            {
                throw new Exception($"Could not find runtime client: {id}");
            }
        }

        public ClientId(int handle)
        {
            Handle = handle;
            //Player = Server.Server.Instance.GetPlayers.FirstOrDefault(x => x.Handle == Handle.ToString());
            if (handle > 0)
                LoadUser();
            Id = User != null ? User.PlayerID : SnowflakeId.Empty;
            //ClientStateBags = new(Player);
            Status = new(Player);
        }

        public ClientId(User user)
        {
            Handle = Convert.ToInt32(user.Player.Handle);
            //Player = user.Player;
            User = user;
            Id = user.PlayerID;
            //ClientStateBags = new(Player);
            Status = new(Player);
        }

        public ClientId(Snowflake id, int handle, string[] identifiers)
        {
            Id = id;
            Handle = handle;
            LoadUser();
            //ClientStateBags = new(Player);
            Status = new(Player);
        }

        public override string ToString()
        {
            return $"{(Id != Snowflake.Empty ? Id.ToString() : Handle.ToString())} ({Player.Name})";
        }

        public bool Compare(Identifiers identifier)
        {
            return Identifiers == identifier;
        }

        public bool Compare(Player player)
        {
            return Compare(player.GetCurrentChar().Identifiers);
        }

        public static explicit operator ClientId(string netId)
        {
            if (int.TryParse(netId.Replace("net:", string.Empty), out int handle))
            {
                return new ClientId(handle);
            }

            throw new Exception($"Could not parse net id: {netId}");
        }

        public bool Compare(ClientId client)
        {
            return client.Handle == Handle;
        }

        public void LoadUser()
        {
            ClientId res;
            res = PluginManager.UserSessions.FirstOrDefault(x => x.Handle == Handle);
            if (res != null)
                User = res.User;
        }

        public static explicit operator ClientId(int handle) => new(handle);
    }
}
