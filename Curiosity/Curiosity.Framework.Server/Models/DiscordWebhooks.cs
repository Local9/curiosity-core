namespace Curiosity.Framework.Server.Models
{
    [DataContract]
    public class DiscordWebhooks
    {
        [DataMember(Name = "server-debug")]
        public string ServerDebug;

        [DataMember(Name = "server-error")]
        public string ServerError;

        [DataMember(Name = "server-player-log")]
        public string ServerPlayerLog;
    }
}