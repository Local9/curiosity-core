using CitizenFX.Core;
using Curiosity.Core.Client.Managers.UI;
using NativeUI;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class SettingsMenu
    {
        private UIMenu settingsMenu;

        UIMenuListItem miDamageEffects;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnListChange += Menu_OnListChange;

            miDamageEffects.Items = DamageEffectManager.GetModule().Effects.Select(x => x.Label).ToList();

            return menu;
        }

        private async void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == miDamageEffects)
            {
                DamageEffectManager damageEffectManager = DamageEffectManager.GetModule();
                dynamic item = damageEffectManager.Effects[newIndex];
                damageEffectManager.Effect = item.Effect;
                await BaseScript.Delay(150);
            }
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
                OnMenuOpen();

            if (state == MenuState.ChangeBackward)
                settingsMenu.InstructionalButtons.Clear();
        }

        private void OnMenuOpen()
        {
            
        }
    }
}
