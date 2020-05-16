using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Shared.Client.net;
using System.Collections.Generic;

namespace Curiosity.Mobile.Client.net.Mobile.Api
{
    class Screen
    {
        public static readonly View LIST = View.Settings;

        // Internal Data.
        protected int id;
        protected string header;
        protected View type;
        protected List<Item> items = new List<Item>();

        public Screen(Application parent, string header, View type)
        {
            id = parent.Screens.Count; // Set it to the index it will be in the list when it is added.
            this.header = header;
            this.type = type;
            parent.AddScreen(this);
        }

        // Setters and getters for internal data.
        public int Id
        {
            get { return id; }
        }
        public string Header
        {
            get { return header; }
        }
        public View Type
        {
            get { return type; }
        }
        public List<Item> Items
        {
            get { return items; }
        }

        // Screen Functions.
        public void AddItem(Item item)
        {
            items.Add(item);
        }
        public void RemoveItem(Item item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
            }
            else
            {
                Log.Error("Could not remove PhoneItem from PhoneScreen.");
            }
        }
        public void ClearItems()
        {
            items.Clear();
        }

    }
}
