namespace Curiosity.Global.Shared.net.Entity
{
    public class TriggerEventForAll
    {
        public int PlayerServerId;
        public string EventName;
        public string Payload;
        public bool passFullSerializedModel;

        public TriggerEventForAll(string eventName, string data)
        {
            EventName = eventName;
            Payload = data;
        }
    }
}
