namespace Curiosity.Framework.Server.Models
{
    [DataContract]
    internal class ServerConfig
    {
        [DataMember(Name = "database")]
        public DatabaseConfig Database;

        [DataMember(Name = "discord")]
        public Discord Discord;
    }
}
