using CitizenFX.Core;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using NativeUI;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

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

            List<dynamic> effects = DamageEffectManager.GetModule().Effects.Select(x => x.Label).ToList();

            int index = 0;
            string savedEffect = GetResourceKvpString("cur:damage:effect");

            Debug.WriteLine(savedEffect);

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == savedEffect)
                {
                    index = i;
                }
            }

            miDamageEffects = new UIMenuListItem("Damage Effect", effects, index);

            menu.AddItem(miDamageEffects);

            return menu;
        }

        private async void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == miDamageEffects)
            {
                DamageEffectManager damageEffectManager = DamageEffectManager.GetModule();
                dynamic item = damageEffectManager.Effects[newIndex];
                damageEffectManager.SetEffect(item.Label, item.Effect);

                NotificationManager.GetModule().Success($"Damage Effect: {item.Label}");
                await BaseScript.Delay(150);
            }
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            //if (state == MenuState.ChangeForward)
            //    OnMenuOpen();

            ////if (state == MenuState.ChangeBackward)
            ////    settingsMenu.InstructionalButtons.Clear();
        }

        private void OnMenuOpen()
        {
            
        }
    }
}
