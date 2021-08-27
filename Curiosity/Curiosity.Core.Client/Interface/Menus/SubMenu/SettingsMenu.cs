using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
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

        UIMenuCheckboxItem miShowServerId;
        UIMenuCheckboxItem miShowPlayerNames;
        UIMenuCheckboxItem miShowMyName;

        UIMenuCheckboxItem miDevEnableGameEventLogger;
        UIMenuCheckboxItem miDevEnableDebugLog;

        PlayerNameManager PlayerNameManager = PlayerNameManager.GetModule();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
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

            miShowServerId = new UIMenuCheckboxItem("Show ServerIDs", PlayerNameManager.ShowServerHandle);
            miShowPlayerNames = new UIMenuCheckboxItem("Show Player Names", PlayerNameManager.ShowPlayerNames);
            miShowMyName = new UIMenuCheckboxItem("Show Own Name", PlayerNameManager.ShowMyName);

            return menu;
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (miDevEnableGameEventLogger is not null && Cache.Player.User.IsDeveloper && checkboxItem == miDevEnableGameEventLogger)
            {
                GameEventManager.GetModule().EnableDebug = Checked;
            }

            if (miDevEnableDebugLog is not null && Cache.Player.User.IsDeveloper && checkboxItem == miDevEnableGameEventLogger)
            {
                Logger.IsDebugEnabled = Checked;
            }

            if (checkboxItem == miShowServerId)
            {
                PlayerNameManager.ShowServerHandle = Checked;
            }

            if (checkboxItem == miShowPlayerNames)
            {
                PlayerNameManager.ShowPlayerNames = Checked;
            }

            if (checkboxItem == miShowMyName)
            {
                PlayerNameManager.ShowMyName = Checked;
            }
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
            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);

            ////if (state == MenuState.ChangeBackward)
            ////    settingsMenu.InstructionalButtons.Clear();
        }

        private void OnMenuOpen(UIMenu menu)
        {
            if (Cache.Player.User.IsDeveloper && miDevEnableGameEventLogger is null)
            {
                if (miDevEnableGameEventLogger is null)
                {
                    miDevEnableGameEventLogger = new UIMenuCheckboxItem("Enable Game Event Logger", GameEventManager.GetModule().EnableDebug);
                    menu.AddItem(miDevEnableGameEventLogger);
                }

                if (miDevEnableDebugLog is null)
                {
                    miDevEnableDebugLog = new UIMenuCheckboxItem("Enable Debug", Logger.IsDebugEnabled);
                    menu.AddItem(miDevEnableDebugLog);
                }
            }
        }
    }
}
