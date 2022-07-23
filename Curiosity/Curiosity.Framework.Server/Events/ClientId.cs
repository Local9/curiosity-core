using Curiosity.Framework.Shared.Models;
using Lusive.Events;
using Lusive.Events.Attributes;
using System.Linq;

namespace Curiosity.Framework.Server.Events
{
    [Serialization]
    public partial class ClientId : ISource
    {
        public int UserId { get; set; }
        public int Handle { get; set; }
        public User User { get; set; }

        [Ignore]
        [JsonIgnore]
        public Player Player { get => PluginManager.PlayerList[Handle]; }

        [Ignore]
        [JsonIgnore]
        public Ped Ped { get => Player.Character; }

        public static readonly ClientId Global = new(-1);

        public ClientId()
        {

        }

        public ClientId(int id)
        {
            Player owner = PluginManager.PlayerList.FirstOrDefault(x => x.Handle == Handle.ToString());
            if (owner != null)
            {
                UserId = id;
                Handle = Convert.ToInt32(owner.Handle);
                LoadUser();
            }
            else
            {
                throw new Exception($"Could not find runtime client: {id}");
            }
        }

        public override string ToString()
        {
            return $"{UserId} ({Player.Name})";
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
            ClientId res = PluginManager.UserSessions[Handle];
            if (res != null)
                User = res.User;
        }

        public static explicit operator ClientId(int handle) => new(handle);
    }
}
