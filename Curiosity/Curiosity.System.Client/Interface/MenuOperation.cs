namespace Curiosity.System.Client.Interface
{
    public class MenuOperation
    {
        public MenuOperationType Type { get; set; }
        public bool IsCancelled { get; set; }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }
}