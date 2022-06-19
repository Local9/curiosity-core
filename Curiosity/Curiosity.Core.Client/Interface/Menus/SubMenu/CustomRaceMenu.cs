using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using NativeUI;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    public class CustomRaceMenu
    {
        private UIMenu menu;

        UIMenuListItem miCreateRace;
        UIMenuListItem miRaceBet;
        List<dynamic> raceList = new List<dynamic>()
        {
            new { label = "Short", minimalBet = 500, maxBet = 5000 },
            new { label = "Normal", minimalBet = 1500, maxBet = 15000 },
            new { label = "Long", minimalBet = 4500, maxBet = 100000 },
        };

        List<dynamic> betAmount = new List<dynamic>()
        {
            500,
            1000,
            1500,
            2000,
            2500,
            3000,
            3500,
            4000,
            4500,
            5000,
            6000,
            7000,
            8000,
            9000,
            10000,
            12500,
            15000,
            25000,
            50000,
            100000,
        };

        int betAmountSelected = 1500;

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;

            miRaceBet = new UIMenuListItem("Race Bet ($)", betAmount.Select(x => x).ToList(), 3);
            miRaceBet.Description = $"Press Enter to set the bet value for the race";
            menu.AddItem(miRaceBet);

            miCreateRace = new UIMenuListItem("Create Race", raceList.Select(x => x.label).ToList(), 1);
            miCreateRace.Description = $"Race for ${betAmountSelected:N0}";
            menu.AddItem(miCreateRace);

            menu.OnListSelect += Menu_OnListSelect;

            return menu;
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == miCreateRace)
            {
                dynamic item = raceList[newIndex];

                int min = item.minimalBet;
                int max = item.maxBet;

                Logger.Debug($"MIN: {min <= (int)betAmountSelected} / {min} | {(int)betAmountSelected}");
                Logger.Debug($"MAX: {(int)betAmountSelected <= max} / {max} | {(int)betAmountSelected}");

                if (min <= (int)betAmountSelected && (int)betAmountSelected <= max)
                {
                    ExecuteCommand($"createrace {item.label} a {betAmountSelected}");
                    InteractionMenu.MenuPool.CloseAllMenus();
                }
                else
                {
                    Notify.Alert($"Current Bet Amount: ~g~${betAmountSelected:N0}");
                    Notify.Alert($"Bet must be between~n~$~g~{item.minimalBet:N0} ~s~& $~g~{item.maxBet:N0}");
                }

            }
            else if (listItem == miRaceBet)
            {
                betAmountSelected = (int)betAmount[newIndex];
                miCreateRace.Description = $"Race for ~g~${betAmountSelected:N0}";
                Screen.ShowNotification($"Race Bet ~g~${betAmountSelected:N0}");
            }
        }
    }
}
