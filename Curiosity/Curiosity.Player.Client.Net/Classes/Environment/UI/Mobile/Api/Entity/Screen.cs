using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api.Entity
{
    class Screen
    {
        int ScreenId;
        int ScreenType;
        string ScreenHeader;
        static Dictionary<int, Item> ScreenItems = new Dictionary<int, Item>();

        public Screen(int id, string header, int type)
        {
            this.ScreenId = id;
            this.ScreenHeader = header;
            this.ScreenType = type;
        }

        public int GetID { get { return ScreenId; } }
        public int GetScreenType { get { return ScreenType; } }

        public void AddCallbackItem(dynamic data, Delegate callback)
        {
            int itemId = ScreenItems.Count + 1;
            ScreenItems.Add(itemId, new Item(itemId, data, callback));
        }

        public void RemoveItem(int itemId)
        {
            ScreenItems.Remove(itemId);
        }

        public void ClearItems()
        {
            ScreenItems.Clear();
        }
    }
}
