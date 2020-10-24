using System;

namespace Curiosity.MissionManager.Shared.EventWrapper
{
    public abstract class BaseEvent
    {
        public string Path { get; }

        protected BaseEvent(string path)
        {
            Path = path;
        }
    }

    public class Event : BaseEvent
    {
        public Action Action { get; set; } = () => { };

        public Event(string path) : base(path)
        {
        }
    }

    public class Event<T> : BaseEvent
    {
        public Action<T> Action { get; set; } = obj => { };
        public Type Field => typeof(T);

        public Event(string path) : base(path)
        {
        }
    }

    public class Event<T1, T2> : BaseEvent
    {
        public Action<T1, T2> Action { get; set; } = (arg1, arg2) => { };
        public Type Field1 => typeof(T1);
        public Type Field2 => typeof(T2);

        public Event(string path) : base(path)
        {
        }
    }

    public class Event<T1, T2, T3> : BaseEvent
    {
        public Action<T1, T2, T3> Action { get; set; } = (arg1, arg2, arg3) => { };
        public Type Field1 => typeof(T1);
        public Type Field2 => typeof(T2);
        public Type Field3 => typeof(T3);

        public Event(string path) : base(path)
        {
        }
    }

    public class Event<T1, T2, T3, T4> : BaseEvent
    {
        public Action<T1, T2, T3, T4> Action { get; set; } = (arg1, arg2, arg3, arg4) => { };
        public Type Field1 => typeof(T1);
        public Type Field2 => typeof(T2);
        public Type Field3 => typeof(T3);
        public Type Field4 => typeof(T4);

        public Event(string path) : base(path)
        {
        }
    }


    public class Event<T1, T2, T3, T4, T5> : BaseEvent
    {
        public Action<T1, T2, T3, T4, T5> Action { get; set; } = (arg1, arg2, arg3, arg4, arg5) => { };
        public Type Field1 => typeof(T1);
        public Type Field2 => typeof(T2);
        public Type Field3 => typeof(T3);
        public Type Field4 => typeof(T4);
        public Type Field5 => typeof(T5);

        public Event(string path) : base(path)
        {
        }
    }

    public class Event<T1, T2, T3, T4, T5, T6> : BaseEvent
    {
        public Action<T1, T2, T3, T4, T5, T6> Action { get; set; } = (arg1, arg2, arg3, arg4, arg5, arg6) => { };
        public Type Field1 => typeof(T1);
        public Type Field2 => typeof(T2);
        public Type Field3 => typeof(T3);
        public Type Field4 => typeof(T4);
        public Type Field5 => typeof(T5);
        public Type Field6 => typeof(T6);

        public Event(string path) : base(path)
        {
        }
    }
}
