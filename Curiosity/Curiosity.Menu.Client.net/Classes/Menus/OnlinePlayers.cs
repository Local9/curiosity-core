//using MenuAPI;
//using System.Threading.Tasks;
//using CitizenFX.Core;
//using CitizenFX.Core.Native;

//namespace Curiosity.Menus.Client.net.Classes.Menus
//{
//    class OnlinePlayers
//    {
//        static Menu menu = new Menu("Online Players", "Online Players");
//        static Client client = Client.GetInstance();

//        private static CitizenFX.Core.Player _currentSpectate = null;
//        private static Vector3 _originalPosition = Vector3.Zero;

//        public static void Init()
//        {
//            MenuBase.AddSubMenu(menu);

//            menu.OnMenuOpen += (_menu) => {

//                foreach (CitizenFX.Core.Player player in Client.players)
//                {
//                    // if (player.ServerId == Game.Player.ServerId) continue;

//                    Menu playerMenu = new Menu(player.Name, "Player Interactions");

//                    playerMenu.OnItemSelect += (_playerMenu, _menuItem, _itemIndex) => {
//                        OnItemSelect(_playerMenu, _menuItem, _itemIndex);
//                    };

//                    Menu reportingOptions = PlayerInteractions.ReportInteraction.CreateMenu("Report", player);
//                    AddSubMenu(playerMenu, reportingOptions);

//                    if (Player.PlayerInformation.IsStaff())
//                    {
//                        playerMenu.AddMenuItem(new MenuItem("Spectate") { ItemData = player, Description = "Spectate player" });
//                        // playerMenu.AddMenuItem(new MenuItem("Bring Player") { ItemData = player, Description = "Teleport player to your location." });
//                        playerMenu.AddMenuItem(new MenuItem("Goto Player") { ItemData = player, Description = "Teleport to a players location." });

//                        Menu kickOptions = PlayerInteractions.KickInteraction.CreateMenu("Kick", player);
//                        AddSubMenu(playerMenu, kickOptions);

//                        Menu banOptions = PlayerInteractions.BanInteraction.CreateMenu("Ban", player);
//                        AddSubMenu(playerMenu, banOptions);
//                    }
//                    AddSubMenu(menu, playerMenu);
//                }
//            };

//            menu.OnMenuOpen += (_menu) =>
//            {
//                MenuBase.MenuOpen(true);
//            };

//            menu.OnMenuClose += (_menu) =>
//            {
//                MenuBase.MenuOpen(false);
//                _menu.ClearMenuItems();
//            };            
//        }

//        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
//        {
//            if (menuItem.Text == "Spectate")
//            {
//                Spectate(menuItem.ItemData);
//            }
//            if (menuItem.Text == "Bring Player")
//            {
//                BringPlayer(menuItem.ItemData);
//            }
//            if (menuItem.Text == "Goto Player")
//            {
//                GotoPlayer(menuItem.ItemData);
//            }
//        }

//        static async Task BringPlayer(CitizenFX.Core.Player player)
//        {
//            if (!Player.PlayerInformation.IsStaff())
//            {
//                Debug.WriteLine("Don't know how you did that...");
//                return;
//            }

//            if (player.ServerId == Game.Player.ServerId)
//            {
//                Debug.WriteLine("Cannot spec yourself");
//                return;
//            }

//            API.NetworkFadeOutEntity(player.Character.Handle, true, false);

//            Vector3 pos = Game.PlayerPed.Position;

//            player.Character.Position = new Vector3(pos.X + 2f, pos.Y, pos.Z);

//            await BaseScript.Delay(50);

//            API.NetworkFadeInEntity(player.Character.Handle, false);

//            await BaseScript.Delay(50);
//        }

//        static async Task GotoPlayer(CitizenFX.Core.Player player)
//        {
//            if (!Player.PlayerInformation.IsStaff())
//            {
//                Debug.WriteLine("Don't know how you did that...");
//                return;
//            }

//            if (player.ServerId == Game.Player.ServerId)
//            {
//                Debug.WriteLine("Cannot teleport to yourself");
//                return;
//            }

//            API.DoScreenFadeOut(200);
//            await BaseScript.Delay(200);

//            API.NetworkFadeOutEntity(Game.PlayerPed.Handle, true, false);

//            await BaseScript.Delay(50);

//            Vector3 pos = player.Character.Position;
//            Game.PlayerPed.Position = new Vector3(pos.X + 2f, pos.Y, pos.Z);

//            await BaseScript.Delay(50);

//            API.NetworkFadeInEntity(Game.PlayerPed.Handle, false);

//            API.DoScreenFadeIn(200);
//            await BaseScript.Delay(50);
//        }

//        static async Task Spectate(CitizenFX.Core.Player player)
//        {
//            if (!Player.PlayerInformation.IsStaff())
//            {
//                Debug.WriteLine("Don't know how you did that...");
//                return;
//            }

//            if (player.ServerId == Game.Player.ServerId)
//            {
//                Debug.WriteLine("Cannot bring yourself to yourself");
//                return;
//            }

//            API.DoScreenFadeOut(200);
//            await BaseScript.Delay(200);

//            int playerPedId = API.GetPlayerPed(player.Handle);

//            if (_currentSpectate != null && _currentSpectate == player)
//            {

//                API.NetworkSetInSpectatorMode(false, playerPedId);

//                // API.FreezeEntityPosition(Game.PlayerPed.Handle, false);
//                API.SetEntityCollision(Game.PlayerPed.Handle, true, true);
//                Game.Player.IsInvincible = false;
//                Game.PlayerPed.IsVisible = true;

//                Game.PlayerPed.Detach();
//                Game.PlayerPed.Position = _originalPosition;

//                _originalPosition = Vector3.Zero;
//                _currentSpectate = null;

//                await BaseScript.Delay(50);

//                API.DoScreenFadeIn(200);

//                return;
//            }

//            API.ClearPlayerWantedLevel(Game.Player.Handle);

//            if (_originalPosition == Vector3.Zero)
//                _originalPosition = Game.PlayerPed.Position;

//            _currentSpectate = player;

//            // API.FreezeEntityPosition(Game.PlayerPed.Handle, true);
//            API.SetEntityCollision(Game.PlayerPed.Handle, false, true);
//            Game.Player.IsInvincible = true;
//            Game.PlayerPed.IsVisible = false;

//            Vector3 entityCords = API.GetEntityCoords(playerPedId, true);

//            API.RequestCollisionAtCoord(entityCords.X, entityCords.Y, entityCords.Z);
//            API.NetworkSetInSpectatorMode(true, playerPedId);

//            API.DoScreenFadeIn(200);
//            await BaseScript.Delay(50);
//        }

//        public static void AddSubMenu(Menu menu, Menu submenu)
//        {
//            MenuController.AddSubmenu(menu, submenu);
//            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→" };
//            menu.AddMenuItem(submenuButton);
//            MenuController.BindMenuItem(menu, submenu, submenuButton);
//        }
//    }
//}
