using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using Curiosity.Systems.Shared.Entity;
using NativeUI;
using System.Linq;
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
        UIMenuItem menuItemStepOutOfTheCar;
        UIMenuItem menuItemRelease;
        UIMenuItem menuItemIdentifcation;
        UIMenuItem menuItemWhatAreYouDoing;
        UIMenuItem menuItemRanRedLight;
        UIMenuItem menuItemSpeeding;
        UIMenuItem menuItemLaneChange;
        UIMenuItem menuItemTailGating;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuItemRelease = new UIMenuItem("You're good to go", "Let the person leave");
            menu.AddItem(menuItemRelease);

            menuItemWelcome = new UIMenuItem("Hi, how are you today?", "Ask Question");
            menu.AddItem(menuItemWelcome);

            menuItemStepOutOfTheCar = new UIMenuItem("Step out of the vehicle for me please", "Ask Question");
            menu.AddItem(menuItemStepOutOfTheCar);

            menuItemIdentifcation = new UIMenuItem("License and Registration please", "Ask Question");
            menu.AddItem(menuItemIdentifcation);

            menuItemWhatAreYouDoing = new UIMenuItem("What are you doing here?", "Ask Question");
            menu.AddItem(menuItemWhatAreYouDoing);

            menuItemRanRedLight = new UIMenuItem("You just ran a red light", "Ask Question");
            menu.AddItem(menuItemRanRedLight);

            menuItemSpeeding = new UIMenuItem("I saw you speeding back there", "Ask Question");
            menu.AddItem(menuItemSpeeding);

            menuItemLaneChange = new UIMenuItem("You changed lanes rather incorrectly", "Ask Question");
            menu.AddItem(menuItemLaneChange);

            menuItemTailGating = new UIMenuItem("I saw you tailgating", "Ask Question");
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

            bool isCalloutActive = Mission.isOnMission;

            if (!isCalloutActive)
            {
                Menu.MenuItems.ForEach(m => m.Enabled = false);
                Ped = null;
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
                menuItemStepOutOfTheCar.Enabled = Ped.Fx.IsInVehicle();

                if (!string.IsNullOrEmpty(Ped.Identity))
                {
                    menuItemIdentifcation.Description = Ped.Identity;
                }

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

            int randomResponse = Decorators.GetInteger(Ped.Handle, Decorators.MENU_RANDOM_RESPONSE);
            bool runFleeChance = false;

            switch(randomResponse)
            {
                case 1:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm fine thanks for asking");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Yea no worries, give me a sec");
                    if (selectedItem == menuItemRelease)
                        ShowPersonSubtitle("Thank you, hope you have a good day");
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
                    if (selectedItem == menuItemStepOutOfTheCar)
                    {
                        runFleeChance = false;
                        ShowPersonSubtitle("Yea, no worries");
                    }
                    break;
                case 2:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm good, whats the problem?");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("I've got it around here somewhere");
                    if (selectedItem == menuItemRelease)
                        ShowPersonSubtitle("Good Bye");
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
                    if (selectedItem == menuItemStepOutOfTheCar)
                    {
                        runFleeChance = false;
                        ShowPersonSubtitle("Why, I've not done anything wrong");
                    }
                    break;
                case 3:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("I'm rather busy, can you make this quick");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Wait, its in my pants");
                    if (selectedItem == menuItemRelease)
                        ShowPersonSubtitle("Now to get to your wifes place");
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
                    if (selectedItem == menuItemStepOutOfTheCar)
                    {
                        runFleeChance = true;
                        ShowPersonSubtitle("Fuck you, I'm not doing anything you pigs say");
                    }
                    break;
                default:
                    if (selectedItem == menuItemWelcome)
                        ShowPersonSubtitle("Leave me alone");
                    if (selectedItem == menuItemIdentifcation)
                        ShowPersonSubtitle("Let me check my ass crack");
                    if (selectedItem == menuItemRelease)
                        ShowPersonSubtitle("Good, F&%* you.");
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
                    if (selectedItem == menuItemStepOutOfTheCar)
                    {
                        runFleeChance = true;
                        ShowPersonSubtitle("If you bend over first, let me grab some butter");
                    }
                    break;
            }

            if (selectedItem == menuItemWelcome)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_WELCOME, true);
            }

            if (selectedItem == menuItemIdentifcation)
            {
                MissionDataPed pedData = await EventSystem.GetModule().Request<MissionDataPed>("mission:ped:identification", Mission.currentMissionData.OwnerHandleId, Ped.NetworkId);
                if (pedData == null)
                {
                    Logger.Error($"Ped Idenfication is null");
                }
                Ped.Identity = $"~b~~h~Identification~h~~w~~n~~b~Name~w~: {pedData.FullName}~n~~b~DoB~w~: {pedData.DateOfBirth.ToString("yyyy-MM-dd")}";
                Screen.ShowNotification(Ped.Identity);
                
                Decorators.Set(Ped.Handle, Decorators.MENU_IDENTIFICATION, true);
                
                menuItemIdentifcation.Description = Ped.Identity;
            }

            if (selectedItem == menuItemWhatAreYouDoing)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_WHAT_YOU_DOING, true);
            }

            if (selectedItem == menuItemRanRedLight)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_RAN_RED_LIGHT, true);
            }

            if (selectedItem == menuItemSpeeding)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_SPEEDING, true);
            }

            if (selectedItem == menuItemLaneChange)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_LANE_CHANGE, true);
            }

            if (selectedItem == menuItemTailGating)
            {
                Decorators.Set(Ped.Handle, Decorators.MENU_TAILGATING, true);
            }

            if (selectedItem == menuItemStepOutOfTheCar)
            {
                if (runFleeChance && Utility.RANDOM.Bool(0.05f))
                {
                    if (Ped.IsDriver)
                    {
                        Ped.RunSequence(Ped.Sequence.FLEE_IN_VEHICLE);
                    }
                    else
                    {
                        Ped.Fx.Weapons.Give(WeaponHash.Pistol, 12, true, true);
                        Ped.Task.ShootAt(Game.PlayerPed, 5000, FiringPattern.Default);
                        
                        await BaseScript.Delay(2000);
                        
                        if (Ped.Fx.CurrentVehicle.Driver != null)
                        {
                            Ped.Fx.CurrentVehicle.Driver.Task.CruiseWithVehicle(Ped.Fx.CurrentVehicle, float.MaxValue,
                                (int)Collections.CombinedVehicleDrivingFlags.Fleeing);
                        }
                    }
                }
                else
                {
                    Ped.RunSequence(Ped.Sequence.LEAVE_VEHICLE);
                }
                menuItemStepOutOfTheCar.Enabled = false;
            }

            if (selectedItem == menuItemRelease)
            {
                if (Ped.IsDriver)
                {
                    if (Ped.IsInVehicle)
                    {
                        Ped.Task.WanderAround();
                    }
                    else
                    {
                        Vehicle veh = Ped.Fx.LastVehicle;
                        Ped.Task.EnterVehicle(veh, VehicleSeat.Driver, 5000);
                        await BaseScript.Delay(5100);
                        Ped.Task.CruiseWithVehicle(veh, 20f, (int)Collections.CombinedVehicleDrivingFlags.Normal);

                        var vehicle = Mission.RegisteredVehicles.Select(x => x).Where(x => x.Handle == veh.Handle).FirstOrDefault();

                        if (vehicle != null)
                        {
                            Mission.RegisteredVehicles.Remove(vehicle);
                        }
                        veh.MarkAsNoLongerNeeded();
                    }
                }
                Ped.MarkAsNoLongerNeeded();
                Ped.IsMission = false;
                Ped.IsSuspect = false;

                Mission.RegisteredPeds.Remove(Ped);

                if (Mission.currentMissionType == MissionType.TrafficStop)
                {
                    Mission.currentMission.Stop(EndState.TrafficStop);
                }
                else
                {
                    Mission.currentMission.Stop(EndState.Unknown);
                }
            }

            await BaseScript.Delay(500);
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
