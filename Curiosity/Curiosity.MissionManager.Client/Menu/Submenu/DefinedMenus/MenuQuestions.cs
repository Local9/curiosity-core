using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Utils;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System.Threading.Tasks;


using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Menu.Submenu.DefinedMenus
{
    class MenuQuestions
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        private Ped Ped;

        UIMenu Menu;
        UIMenuItem menuItemWelcome;
        UIMenuItem menuItemIdentifcation;
        UIMenuItem menuItemWhatAreYouDoing;
        UIMenuItem menuItemRanRedLight;
        UIMenuItem menuItemSpeeding;
        UIMenuItem menuItemLaneChange;
        UIMenuItem menuItemTailGating;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemWelcome = new UIMenuItem("Hi, how are you today?");
            menu.AddItem(menuItemWelcome);

            menuItemIdentifcation = new UIMenuItem("License and Registration");
            menu.AddItem(menuItemIdentifcation);

            menuItemWhatAreYouDoing = new UIMenuItem("What are you doing here?");
            menu.AddItem(menuItemWhatAreYouDoing);

            menuItemRanRedLight = new UIMenuItem("You just ran a red light");
            menu.AddItem(menuItemRanRedLight);

            menuItemSpeeding = new UIMenuItem("I saw you speeding back there");
            menu.AddItem(menuItemSpeeding);

            menuItemLaneChange = new UIMenuItem("You changed lanes rather incorrectly");
            menu.AddItem(menuItemLaneChange);

            menuItemTailGating = new UIMenuItem("I saw you tailgating");
            menu.AddItem(menuItemTailGating);

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            Menu = menu;
            return menu;
        }

        private void Menu_OnMenuClose(UIMenu sender)
        {
            MenuManager.OnMenuState();
            PluginInstance.DetachTickHandler(OnSuspectDistanceCheck);
        }

        private void Menu_OnMenuOpen(UIMenu sender)
        {
            MenuManager.OnMenuState(true);

            if (Game.PlayerPed.IsInVehicle())
            {
                MenuManager._MenuPool.CloseAllMenus();

                Notify.Alert(CommonErrors.OutsideVehicle);

                Menu.MenuItems.ForEach(m => m.Enabled = false);

                return;
            }

            bool isCalloutActive = MenuManager.IsCalloutActive;

            if (!isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => m.Enabled = false);
            }
            else
            {
                Ped = MenuManager.GetClosestInteractivePed();
                bool isControlable = PedCanBeControled();

                if (Ped == null)
                {
                    Notify.Alert(CommonErrors.MustBeCloserToSubject);
                    MenuManager._MenuPool.CloseAllMenus();
                    Menu.MenuItems.ForEach(m => m.Enabled = false);
                    return;
                }

                menuItemWelcome.Enabled = isControlable;
                menuItemIdentifcation.Enabled = isControlable;
                menuItemWhatAreYouDoing.Enabled = isControlable;

                menuItemRanRedLight.Enabled = Ped.IsDriver;
                menuItemSpeeding.Enabled = Ped.IsDriver;
                menuItemLaneChange.Enabled = Ped.IsDriver;
                menuItemTailGating.Enabled = Ped.IsDriver;

                PluginInstance.AttachTickHandler(OnSuspectDistanceCheck);
            }
        }

        private async Task OnSuspectDistanceCheck()
        {
            if (Ped.Position.Distance(Game.PlayerPed.Position) > 3f)
                MenuManager._MenuPool.CloseAllMenus();
        }

        private async void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (Ped == null)
            {
                Notify.Alert(CommonErrors.SubjectNotFound);
                Menu.MenuItems.ForEach(m => m.Enabled = false);
                return;
            }

            int randomResponse = Utility.RANDOM.Next(1, 4);

            switch(randomResponse)
            {
                case 1:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm fine thanks for asking");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Yea no worries, give me a sec");
                    if (selectedItem == menuItemWhatAreYouDoing)
                        ShowPersonSubtitle("I live around here");
                    if (selectedItem == menuItemRanRedLight)
                        ShowPersonSubtitle("Sorry, I honestly thought it was yellow");
                    if (selectedItem == menuItemSpeeding)
                        ShowPersonSubtitle("I don't think I was going that fast");
                    if (selectedItem == menuItemLaneChange)
                        ShowPersonSubtitle("No, I think you're wrong");
                    if (selectedItem == menuItemTailGating)
                        ShowPersonSubtitle("Oh, my bad. I'll back off more in the future");
                    break;
                case 2:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm good, whats the problem?");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("I've got it around here somewhere");
                    if (selectedItem == menuItemWhatAreYouDoing)
                        ShowPersonSubtitle("I've gotten lost, I don't really know the area");
                    if (selectedItem == menuItemRanRedLight)
                        ShowPersonSubtitle("You sure it was red?");
                    if (selectedItem == menuItemSpeeding)
                        ShowPersonSubtitle("Sorry, I'm in a hurry you see");
                    if (selectedItem == menuItemLaneChange)
                        ShowPersonSubtitle("I needed to get to the other lane else I would of gone the wrong way");
                    if (selectedItem == menuItemTailGating)
                        ShowPersonSubtitle("I was following my buddy, now I'm going to lose them");
                    break;
                case 3:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm rather busy, can you make this quick");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Wait, its in my pants");
                    if (selectedItem == menuItemWhatAreYouDoing)
                        ShowPersonSubtitle("Waiting for my friend, what business is it of yours?");
                    if (selectedItem == menuItemRanRedLight)
                        ShowPersonSubtitle("Nope, green light is always at the top");
                    if (selectedItem == menuItemSpeeding)
                        ShowPersonSubtitle("I was keeping up with the traffic, why didn't you pull them over");
                    if (selectedItem == menuItemLaneChange)
                        ShowPersonSubtitle("They were cutting into me so I changed lanes");
                    if (selectedItem == menuItemTailGating)
                        ShowPersonSubtitle("Are you serious right now? Really, you pull me over for that?!");
                    break;
                default:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("Leave me alone");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Let me check my ass crack");
                    if (selectedItem == menuItemWhatAreYouDoing)
                        ShowPersonSubtitle("Watching your mother undress, what you going to do about it?");
                    if (selectedItem == menuItemRanRedLight)
                        ShowPersonSubtitle("What traffic lights? I'm rather blind when pigs are near me");
                    if (selectedItem == menuItemSpeeding)
                        ShowPersonSubtitle("I was rushing to your wifes place, she said you were out");
                    if (selectedItem == menuItemLaneChange)
                        ShowPersonSubtitle("What lane? theres no lanes here, its mercia!");
                    if (selectedItem == menuItemTailGating)
                        ShowPersonSubtitle("I'll bend you over and tailgate you");
                    break;
            }

            if (selectedItem == menuItemIdentifcation)
            {
                MissionDataPed pedData = await EventSystem.GetModule().Request<MissionDataPed>("mission:ped:identification", Mission.currentMissionData.OwnerHandleId, Ped.NetworkId);

                Screen.ShowNotification($"~b~~h~Identification~h~~w~~n~~b~Name~w~: {pedData.FullName}~n~~b~DoB~w~: {pedData.DateOfBirth.ToString("yyyy-MM-dd")}");
                // Server: name, age, address
                // need to add them to the ped model on the server
            }
        }

        private void ShowPersonSubtitle(string message)
        {
            Screen.ShowSubtitle($"~b~Person~w~: {message}");
        }

        private bool PedCanBeControled()
        {
            if (Ped != null)
            {
                if (Ped.Exists())
                    return Ped.IsAlive;
            }
            return false;
        }
    }
}
