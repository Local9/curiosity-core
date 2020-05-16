using Atlas.Roleplay.Client.Interface.Impl;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Interface
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public Menu MenuContext { get; set; }

        public override void Begin()
        {
            Atlas.AttachNuiHandler("MENU_DIALOG_UPDATE", new EventCallback(metadata =>
            {
                if (MenuContext?.Profile != null && MenuContext.Profile.GetType() == typeof(MenuProfileDialog))
                    ((MenuProfileDialog)MenuContext.Profile).Value = metadata.Find<string>(0);

                return null;
            }));

            Atlas.AttachNuiHandler("MENU_DIALOG_SELECT", new EventCallback(metadata =>
            {
                if (MenuContext?.Profile == null || MenuContext.Profile.GetType() != typeof(MenuProfileDialog))
                    return null;

                var operation = new MenuOperation
                {
                    Type = MenuOperationType.Select
                };

                MenuContext.Callback?.Invoke(MenuContext, null, operation);

                return null;
            }));
        }

        public async Task MenuUpdateTask()
        {
            if (MenuContext == null || MenuContext?.Type == 1)
            {
                await BaseScript.Delay(100);

                return;
            }

            if (Game.IsControlJustPressed(0, Control.PhoneSelect))
            {
                var operation = new MenuOperation
                {
                    Type = MenuOperationType.Select
                };

                MenuContext.Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex), operation));
                MenuContext.Callback?.Invoke(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex),
                    operation);
            }
            else if (Game.IsControlJustPressed(0, Control.PhoneUp))
            {
                MenuContext.ItemIndex =
                    MenuContext.ItemIndex <= 0 ? MenuContext.Items.Count - 1 : MenuContext.ItemIndex - 1;

                var operation = new MenuOperation
                {
                    Type = MenuOperationType.Update
                };

                MenuContext.Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex), operation));
                MenuContext.Callback?.Invoke(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex),
                    operation);

                if (!operation.IsCancelled)
                    API.SendNuiMessage(new JsonBuilder().Add("Operation", "UPDATE_MENU").Add("Metadata", MenuContext)
                        .Build());
            }
            else if (Game.IsControlJustPressed(0, Control.PhoneDown))
            {
                MenuContext.ItemIndex =
                    MenuContext.ItemIndex >= MenuContext.Items.Count - 1 ? 0 : MenuContext.ItemIndex + 1;

                var operation = new MenuOperation
                {
                    Type = MenuOperationType.Update
                };

                MenuContext.Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex), operation));
                MenuContext.Callback?.Invoke(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex),
                    operation);

                if (!operation.IsCancelled)
                    API.SendNuiMessage(new JsonBuilder().Add("Operation", "UPDATE_MENU").Add("Metadata", MenuContext)
                        .Build());
            }
            else if (Game.IsControlJustPressed(0, Control.PhoneCancel))
            {
                var operation = new MenuOperation
                {
                    Type = MenuOperationType.Close
                };

                MenuContext.Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex), operation));
                MenuContext.Callback?.Invoke(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex),
                    operation);

                if (!operation.IsCancelled) InterfaceManager.GetModule().MenuContext.Hide();
            }
            else
            {
                var left = Game.IsControlJustPressed(0, Control.PhoneLeft);
                var right = Game.IsControlJustPressed(0, Control.PhoneRight);

                if (!left && !right) return;

                var item = MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex);

                if (item?.Profile == null || item.Profile.GetType() != typeof(MenuProfileSlider)) return;

                var profile = (MenuProfileSlider)item.Profile;

                if (left)
                    profile.Current = profile.Current <= profile.Minimum ? profile.Maximum : profile.Current - 1;
                else
                    profile.Current = profile.Current >= profile.Maximum ? profile.Minimum : profile.Current + 1;

                var operation = new MenuSliderOperation
                {
                    Type = MenuOperationType.SliderUpdate,
                    Current = profile.Current,
                    Item = item
                };

                MenuContext.Items.Where(self => self.Profile != null).Select(self => self.Profile).ToList().ForEach(self => self.On(MenuContext, MenuContext.Items.ElementAtOrDefault(MenuContext.ItemIndex), operation));
                MenuContext.Callback?.Invoke(MenuContext, item,
                    operation);

                if (operation.IsCancelled) return;

                item.SecondaryLabel = $"- {profile.Current} -";

                MenuContext.Commit();
            }

            await BaseScript.Delay(0);
        }
    }
}