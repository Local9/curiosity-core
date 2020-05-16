using CitizenFX.Core.Native;
using Curiosity.System.Client.Interface.Impl;
using Curiosity.System.Library.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Curiosity.System.Client.Interface
{
    public class Menu : MenuMetadata
    {
        [JsonIgnore] public Action<Menu, MenuItem, MenuOperation> Callback { get; set; }
        public IMenuProfile Profile { get; set; }

        public Menu(string header)
        {
            Header = header;
        }

        [JsonIgnore] public MenuItem Selected => Items.ElementAtOrDefault(ItemIndex);

        public int Type => Profile != null && Profile.GetType() == typeof(MenuProfileDialog) ? 1 : 0;

        public void Commit()
        {
            var atlas = CuriosityPlugin.Instance;
            var manager = InterfaceManager.GetModule();
            var context = manager.MenuContext;

            foreach (var item in Items)
            {
                item.Profile?.Begin(manager.MenuContext, item);
            }

            if (context != null)
            {
                manager.MenuContext = this;

                API.SendNuiMessage(new JsonBuilder().Add("Operation", "UPDATE_MENU").Add("Metadata", this).Build());
            }
            else
            {
                manager.MenuContext = this;
                atlas.AttachTickHandler(manager.MenuUpdateTask);

                API.SendNuiMessage(new JsonBuilder().Add("Operation", "OPEN_MENU").Add("Metadata", this).Build());
            }

            if (manager.MenuContext.Type == 1)
            {
                API.SetNuiFocus(true, false);
            }
        }

        public void Hide(bool sendOperation = true)
        {
            var atlas = CuriosityPlugin.Instance;
            var manager = InterfaceManager.GetModule();

            if (manager.MenuContext == null) return;

            atlas.DetachTickHandler(manager.MenuUpdateTask);

            API.SendNuiMessage(new JsonBuilder().Add("Operation", "CLOSE_MENU").Build());

            if (manager.MenuContext.Type == 1)
            {
                API.SetNuiFocus(false, false);
            }

            manager.MenuContext = null;

            if (!sendOperation) return;

            var operation = new MenuOperation
            {
                Type = MenuOperationType.PostClose
            };

            Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(this, Items.ElementAtOrDefault(ItemIndex), operation));

            Callback?.Invoke(this, Items.ElementAtOrDefault(ItemIndex), operation);
        }
    }
}