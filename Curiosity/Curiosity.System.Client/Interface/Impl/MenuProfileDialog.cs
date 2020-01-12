namespace Curiosity.System.Client.Interface.Impl
{
    public class MenuProfileDialog : IMenuProfile
    {
        public string Standard { get; set; }
        public string Value { get; set; }

        public MenuProfileDialog(string standard)
        {
            Standard = standard;
            Value = standard;
        }

        public void Begin(Menu menu)
        {
            // Ignored
        }
    }
}