using CitizenFX.Core;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Client.net.Classes.Menus
{
    class PlayerActionsMenu
    {
        static Client client = Client.GetInstance();

        public static MenuObserver Observer;
        public static MenuModel PlayerActionsMenuModel;
        public static MenuModel PlayerSubActionsMenu;
        public static MenuModel BanActionSubMenu;
        public static MenuModel KickActionSubMenu;
        public static List<Tuple<int, MenuItem, Func<bool>>> ItemsAll = new List<Tuple<int, MenuItem, Func<bool>>>();
        internal static List<MenuItem> ItemsFiltered = new List<MenuItem>();
        public static bool IsDirty = false;

        static string playerHandle;
        static string playerName;
        static bool isListSetup = false;

        static List<GlobalEntities.LogType> banReasons = new List<GlobalEntities.LogType>();
        static List<GlobalEntities.LogType> kickReasons = new List<GlobalEntities.LogType>();

        static string FormatName()
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return string.Empty;
            }
            return playerName.Length > 20 ? string.Format("{0}...", playerName.Substring(0, 20)) : playerName;
        }

        public static void Init()
        {
            try
            {
                client.RegisterEventHandler("curiosity:Client:Menu:Ban", new Action<string>(SetupBanReasons));
                client.RegisterEventHandler("curiosity:Client:Menu:Kick", new Action<string>(SetupKickReasons));

                client.RegisterTickHandler(OnTick);

                PlayerActionsMenuModel = new PlayerMenu { numVisibleItems = 7 };
                PlayerActionsMenuModel.headerTitle = "Players";
                PlayerActionsMenuModel.statusTitle = "";
                PlayerActionsMenuModel.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loading..." } };

                InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
                {
                    Title = $"Player Actions",
                    SubMenu = PlayerActionsMenuModel
                }, () => true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"{ex.InnerException}");

            }
        }

        static async Task OnTick()
        {
            if (Player.PlayerInformation.IsStaff())
            {
                while (!isListSetup)
                {
                    await Client.Delay(1000);
                    Client.TriggerServerEvent("curiosity:Server:Menu:Reasons", (int)GlobalEnums.LogGroup.Ban);
                    await Client.Delay(1000);
                    Client.TriggerServerEvent("curiosity:Server:Menu:Reasons", (int)GlobalEnums.LogGroup.Kick);
                    if (banReasons.Count > 0)
                    {
                        isListSetup = true;
                        CitizenFX.Core.UI.Screen.ShowNotification("~b~Staff Actions: ~g~Configured");
                    }
                }
            }
            await Task.FromResult(0);
        }

        static async void SetupBanReasons(string json)
        {
            banReasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GlobalEntities.LogType>>(json);
        }

        static async void SetupKickReasons(string json)
        {
            kickReasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GlobalEntities.LogType>>(json);
        }

        class BanPlayerMenu : MenuModel
        {
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();

                if (Player.PlayerInformation.IsStaff())
                {
                    if (string.IsNullOrEmpty(playerHandle))
                    {
                        _menuItems.Add(new MenuItemBack { Title = "Player Handle Missing" });
                    }
                    else
                    {
                        foreach (GlobalEntities.LogType logType in banReasons)
                        {
                            _menuItems.Add(new MenuItemStandard
                            {
                                Title = logType.Description
                                ,
                                MetaData = $"{logType.LogTypeId}"
                                ,
                                OnActivate = (item) =>
                              {
                                  Screen.ShowNotification($"Test ban for {item.MetaData} on {playerName}");
                              }
                            });
                        }
                    }
                }

                menuItems = _menuItems;
            }
        }

        class KickPlayerMenu : MenuModel
        {
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();

                if (Player.PlayerInformation.IsStaff())
                {
                    if (string.IsNullOrEmpty(playerHandle))
                    {
                        _menuItems.Add(new MenuItemBack { Title = "Player Handle Missing" });
                    }
                    else
                    {
                        foreach (GlobalEntities.LogType logType in kickReasons)
                        {
                            _menuItems.Add(new MenuItemStandard
                            {
                                Title = logType.Description
                                ,
                                MetaData = $"{logType.LogTypeId}"
                                ,
                                OnActivate = (item) =>
                              {
                                  Screen.ShowNotification($"Test kick for {item.MetaData} on {playerName}");
                              }
                            });
                        }
                    }
                }

                menuItems = _menuItems;
            }
        }

        class SubActionsMenu : MenuModel
        {
            public override void Refresh()
            {
                BanActionSubMenu = new BanPlayerMenu { numVisibleItems = 7 };
                BanActionSubMenu.headerTitle = $"Ban {FormatName()}";
                BanActionSubMenu.statusTitle = "";
                BanActionSubMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loading..." } };

                KickActionSubMenu = new KickPlayerMenu { numVisibleItems = 7 };
                KickActionSubMenu.headerTitle = $"Kick {FormatName()}";
                KickActionSubMenu.statusTitle = "";
                KickActionSubMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loading..." } };

                var _menuItems = new List<MenuItem>();

                if (Player.PlayerInformation.IsStaff())
                {
                    _menuItems.Add(new MenuItemSubMenu { Title = "Kick", SubMenu = KickActionSubMenu });
                    _menuItems.Add(new MenuItemSubMenu { Title = "Ban", SubMenu = BanActionSubMenu });
                }

                _menuItems.Add(new MenuItemStandard { Title = "Invite to Party", Description = "Coming Soon™" });

                menuItems = _menuItems;
            }
        }

        class PlayerMenu : MenuModel
        {
            public override void Refresh()
            {
                PlayerSubActionsMenu = new SubActionsMenu { numVisibleItems = 7 };
                PlayerSubActionsMenu.headerTitle = $"Actions";
                PlayerSubActionsMenu.statusTitle = "";
                PlayerSubActionsMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Loading..." } };

                var _menuItems = new List<MenuItem>();

                _menuItems.Add(new MenuItemBack { Title = "Go Back", OnSelect = (item) => { playerName = string.Empty; playerHandle = string.Empty; } });

                foreach(CitizenFX.Core.Player player in new PlayerList())
                {
                    _menuItems.Add(new MenuItemSubMenu
                    {
                        Title = $"{player.ServerId}: {player.Name}"
                        , SubMenu = PlayerSubActionsMenu
                        , OnSelect = (item) =>
                        {
                            if (item.Title.Contains(":"))
                            {
                                string[] str = item.Title.Split(':');

                                playerName = str[1];
                                playerHandle = str[0];
                            }
                            else
                            {
                                playerName = string.Empty;
                                playerHandle = string.Empty;
                            }
                        }
                    });
                }

                menuItems = _menuItems;
            }
        }
    }
}
