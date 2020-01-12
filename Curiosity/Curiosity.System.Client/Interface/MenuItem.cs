namespace Curiosity.System.Client.Interface
{
    public class MenuItem
    {
        public string Seed { get; set; }
        public string Label { get; set; }
        public string SecondaryLabel { get; set; }
        public IMenuItemProfile Profile { get; set; }
        public object[] Metadata { get; set; } = new object[0];

        public MenuItem(string seed, string label, params object[] metadata)
        {
            Seed = seed;
            Label = label;
            Metadata = metadata;
        }

        public MenuItem(string seed, string label)
        {
            Seed = seed;
            Label = label;
        }
    }
}