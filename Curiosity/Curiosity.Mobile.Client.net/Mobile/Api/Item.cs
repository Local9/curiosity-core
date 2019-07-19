using System;
using System.Collections.Generic;

namespace Curiosity.Mobile.Client.net.Mobile.Api
{
    class Item
    {
        // Internal Data.
        protected int id;
        protected List<ItemData> data = new List<ItemData>(); // I have no idea how this works.
        protected Action<dynamic[]> callback;
        protected dynamic[] args;

        public Item(Screen parent, List<ItemData> data, Action<dynamic[]> callback, params dynamic[] args)
        {
            id = parent.Items.Count; // Set to index this item will be when it's added.
            this.data = data;
            this.callback = callback;
            this.args = args;
        }

        // Setters and getters for internal data.
        public int Id
        {
            get { return id; }
        }
        public List<ItemData> Data
        {
            get { return data; }
        }

        // PhoneItem Functions.
        public void Select()
        {
            callback.Invoke(args);
        }

        public static List<ItemData> CreateData(params dynamic[] args)
        {
            List<ItemData> data = new List<ItemData>();
            foreach (dynamic value in args)
            {
                data.Add(new ItemData(value));
            }
            return data;
        }
    }

}
