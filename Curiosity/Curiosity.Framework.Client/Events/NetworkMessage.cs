﻿using Lusive.Snowflake;

namespace Curiosity.Framework.Client.Events
{
    public class NetworkMessage
    {
        public SnowflakeId Id { get; set; }
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public long Timestamp { get; set; }
        public object Payload { get; set; }
        public bool HasResponded { get; set; }
        public object Response { get; set; }
        public long? ResponseTime { get; set; }

        public NetworkMessage(SnowflakeId id, string method, string endpoint, long timestamp, object payload)
        {
            Id = id;
            Method = method;
            Endpoint = endpoint;
            Timestamp = timestamp;
            Payload = payload;
        }
    }
}
