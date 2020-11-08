using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Threading
{
    public class ThreadLock
    {
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public Task Wait()
        {
            return _tcs.Task;
        }

        public void Unlock()
        {
            _tcs.TrySetResult(true);
        }
    }
}