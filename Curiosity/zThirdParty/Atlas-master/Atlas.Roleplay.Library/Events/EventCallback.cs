using System;

namespace Atlas.Roleplay.Library.Events
{
    public class EventCallback
    {
        public Func<EventMetadata, object> Task { get; set; }

        public EventCallback(Func<EventMetadata, object> task)
        {
            Task = task;
        }
    }
}