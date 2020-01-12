namespace Curiosity.System.Client.Interface
{
    public interface IMenuItemProfile
    {
        void Begin(Menu menu, MenuItem item);
        void On(Menu menu, MenuItem item, MenuOperation operation);
    }
}