using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Curiosity.Systems.Library.Utils
{
    public class RealTimer
    {
        public DateTime Start;

        public RealTimer(DateTime _start) => this.Start = _start;

        public RealTimer() => this.Start = DateTime.Now;

        public void Reset([DateTimeConstant(0), Optional] DateTime _start)
        {
            if (DateTime.Compare(_start, DateTime.MinValue) == 0)
                _start = DateTime.Now;
            this.Start = _start;
        }

        public bool TotalSeconds(int Span) => (DateTime.Now - this.Start).TotalSeconds > (double)Span;

        public bool TotalMilliseconds(int Span) => (DateTime.Now - this.Start).TotalMilliseconds > (double)Span;

        public bool TotalMinutes(int Span) => (DateTime.Now - this.Start).TotalMinutes > (double)Span;

        public bool TotalHours(int Span) => (DateTime.Now - this.Start).TotalHours > (double)Span;
    }
}
