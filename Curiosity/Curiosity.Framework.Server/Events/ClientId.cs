using Curiosity.Framework.Server.Models.Database;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents.Shared.EventSubsystem;

namespace Curiosity.Framework.Server.Events
{
    public partial class ClientId : ISource
    {
        public int UserId { get; set; }
        public int Handle { get; set; }
        public User User { get; set; }
        internal DataStoreUser StoreUser { get; set; }
        internal Player Player { get => PluginManager.PlayerList[Handle]; }
        internal Ped Ped { get => Player.Character; }
        public static readonly ClientId Global = new(-1);

        public ClientId()
        {

        }

        public ClientId(int handle)
        {
            Handle = handle;
            if (handle > 0)
                LoadUser();
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
            if (!PluginManager.UserSessions.ContainsKey(Handle)) return;

            ClientId res = PluginManager.UserSessions[Handle];
            if (res != null)
                User = res.User;
        }

        public static explicit operator ClientId(int handle) => new(handle);
    }
}
