namespace Curiosity.Framework.Server.Models
{
    [DataContract]
    public class DatabaseConfig
    {
        [DataMember(Name = "server")]
        public string Server;

        [DataMember(Name = "databaseName")]
        public string DatabaseName;

        [DataMember(Name = "port")]
        public uint Port;

        [DataMember(Name = "username")]
        public string Username;

        [DataMember(Name = "password")]
        public string Password;

        [DataMember(Name = "minimumPoolSize")]
        public uint MinimumPoolSize { get; set; } = 10;

        [DataMember(Name = "maximumPoolSize")]
        public uint MaximumPoolSize { get; set; } = 50;

        [DataMember(Name = "connectionTimeout")]
        public uint ConnectionTimeout { get; set; } = 5;

    }
}