using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Events
{
    public class EventRequest : Event
    {
        [JsonIgnore] public EventCallback Callback { get; set; }
        [JsonIgnore] public bool IsPending { get; set; }

        public EventRequest(EventCallback callback)
        {
            Callback = callback;
            Type = EventType.Request;
        }
    }
}