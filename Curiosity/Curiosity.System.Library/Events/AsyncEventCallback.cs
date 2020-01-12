using System;
using System.Threading.Tasks;

namespace Curiosity.System.Library.Events
{
    public class AsyncEventCallback : EventCallback
    {
        public Func<EventMetadata, Task<object>> AsyncTask { get; set; }

        public AsyncEventCallback(Func<EventMetadata, Task<object>> task) : base(null)
        {
            AsyncTask = task;
        }
    }
}