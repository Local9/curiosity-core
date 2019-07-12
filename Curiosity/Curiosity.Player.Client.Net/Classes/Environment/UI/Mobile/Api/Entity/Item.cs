using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api.Entity
{
    class Item
    {
        int Id;
        dynamic Data;
        Delegate CallBack;

        public Item(int id, dynamic data, Delegate callback)
        {
            this.Id = id;
            this.Data = data;
            this.CallBack = callback;
        }

        public int GetId { get { return Id; } }
        public dynamic GetData { get { return Data; } }
        public Delegate GetCallback { get { return CallBack; } }
    }
}
