using Lusive.Events.Attributes;
using Lusive.Snowflake;

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class User
    {
        public int Handle;
        public string Username;
        public SnowflakeId PlayerID;

        public User()
        {

        }

        public User(int handle, string username)
        {
            Handle = handle;
            Username = username;
        }
    }
}
