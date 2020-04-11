using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using MenuAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class OnlinePlayers
    {
        static Menu menu = new Menu("Online Players", "Online Players");
        static Client client = Client.GetInstance();

        private static CitizenFX.Core.Player _currentSpectate = null;
        private static Vector3 _originalPosition = Vector3.Zero;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:Bring", new Action<string>(OnBringPlayer));
            client.RegisterEventHandler("curiosity:Client:Player:Freeze", new Action<string>(OnFreezePlayer));

            menu.OnMenuOpen += (_menu) => {
                MenuBase.MenuOpen(true);

                PlayerList players = Client.players;

                foreach (CitizenFX.Core.Player player in players.OrderBy(p => p.Name))
                {
                    if (!Player.PlayerInformation.IsDeveloper())
                    {
                        if (player.ServerId == Game.Player.ServerId) continue;
                    }

                    Menu playerMenu = new Menu($"[{player.ServerId}] {player.Name}", "Player Interactions");

                    playerMenu.OnMenuOpen += (_m) =>
                    {
                        MenuBase.MenuOpen(true);
                    };

                    playerMenu.OnMenuClose += (_m) =>
                    {
                        MenuBase.MenuOpen(false);
                        _m.ClearMenuItems();
                    };

                    playerMenu.OnItemSelect += (_playerMenu, _menuItem, _itemIndex) => {
                        OnItemSelect(_playerMenu, _menuItem, _itemIndex);
                    };

                    MenuItem messagePlayer = new MenuItem("Message") { Enabled = false, Description = "Coming Soon", RightIcon = MenuItem.Icon.LOCK };
                    playerMenu.AddMenuItem(messagePlayer);

                    Menu reportingOptions = PlayerInteractions.ReportInteraction.CreateMenu("Report", player);
                    AddSubMenu(playerMenu, reportingOptions);

                    if (Player.PlayerInformation.IsStaff())
                    {
                        playerMenu.AddMenuItem(new MenuItem("-- ADMIN TOOLS --") { Enabled = false, LeftIcon = MenuItem.Icon.STAR });
                        playerMenu.AddMenuItem(new MenuItem("Spectate") { ItemData = player, Description = "Spectate player" });
                        playerMenu.AddMenuItem(new MenuItem("Bring Player") { ItemData = player, Description = "Teleport player to your location" });
                        playerMenu.AddMenuItem(new MenuItem("Goto Player") { ItemData = player, Description = "Teleport to a players location" });
                        playerMenu.AddMenuItem(new MenuItem("Freeze Player") { ItemData = player, Description = "Freeze players location" });

                        Menu kickOptions = PlayerInteractions.KickInteraction.CreateMenu("Kick", player);
                        AddSubMenu(playerMenu, kickOptions);

                        Menu banOptions = PlayerInteractions.BanInteraction.CreateMenu("Ban", player);
                        AddSubMenu(playerMenu, banOptions);
                    }
                    AddSubMenu(menu, playerMenu);
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                MenuBase.MenuOpen(false);
                _menu.ClearMenuItems();
            };

            MenuBase.AddSubMenu(menu, leftIcon: MenuItem.Icon.INV_PERSON);
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem.Text == "Spectate")
            {
                Spectate(menuItem.ItemData);
            }
            if (menuItem.Text == "Bring Player")
            {
                BringPlayer(menuItem.ItemData);
            }
            if (menuItem.Text == "Goto Player")
            {
                GotoPlayer(menuItem.ItemData);
            }
            if (menuItem.Text == "Freeze Player")
            {
                FreezePlayer(menuItem.ItemData);
            }
        }

        static async void BringPlayer(CitizenFX.Core.Player player)
        {
            if (!Player.PlayerInformation.IsStaff())
            {
                Debug.WriteLine("Don't know how you did that...");
                return;
            }

            if (player.ServerId == Game.Player.ServerId)
            {
                Debug.WriteLine("Cannot teleport to yourself");
                return;
            }

            Vector3 pos = Game.PlayerPed.Position;

            Client.TriggerServerEvent("curiosity:Server:Player:Bring", player.ServerId, pos.X, pos.Y, pos.Z);

            await BaseScript.Delay(50);
        }

        static async void FreezePlayer(CitizenFX.Core.Player player)
        {
            try
            {
                if (!Player.PlayerInformation.IsStaff())
                {
                    Debug.WriteLine("Don't know how you did that...");
                    return;
                }

                if (player.ServerId == Game.Player.ServerId)
                {
                    Debug.WriteLine("Cannot freeze yourself");
                    return;
                }

                Client.TriggerServerEvent("curiosity:Server:Player:Freeze", player.ServerId);
            }
            catch (Exception ex)
            {
                // 
            }

            await BaseScript.Delay(50);
        }

        static async void OnBringPlayer(string data)
        {
            try
            {
                string json = Encode.BytesToStringConverted(Convert.FromBase64String(data));

                GenericData genericData = Newtonsoft.Json.JsonConvert.DeserializeObject<GenericData>(json);

                Screen.Fading.FadeOut(200);
                while (Screen.Fading.IsFadingOut)
                {
                    await Client.Delay(10);
                }

                await Client.Delay(0);
                Game.PlayerPed.Position = new Vector3(genericData.X, genericData.Y, genericData.Z);
                await Client.Delay(0);

                Screen.Fading.FadeIn(200);
                while (Screen.Fading.IsFadingIn)
                {
                    await Client.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Game.PlayerPed.IsPositionFrozen = false;
                Screen.Fading.FadeOut(10);
            }
        }

        static void OnFreezePlayer(string data)
        {
            try
            {
                string json = Encode.BytesToStringConverted(Convert.FromBase64String(data));
                GenericData genericData = Newtonsoft.Json.JsonConvert.DeserializeObject<GenericData>(json);

                if (genericData.IsSentByServer)
                {
                    bool handcuff = !Game.PlayerPed.IsPositionFrozen;
                    Game.PlayerPed.IsPositionFrozen = handcuff;
                    API.SetEnableHandcuffs(Game.PlayerPed.Handle, handcuff);
                }
            }
            catch (Exception ex)
            {
                Game.PlayerPed.IsPositionFrozen = false;
            }
        }

        static async Task GotoPlayer(CitizenFX.Core.Player player)
        {
            if (!Player.PlayerInformation.IsStaff())
            {
                Debug.WriteLine("Don't know how you did that...");
                return;
            }

            if (player.ServerId == Game.Player.ServerId)
            {
                Debug.WriteLine("Cannot teleport to yourself");
                return;
            }

            API.DoScreenFadeOut(200);
            await BaseScript.Delay(200);

            API.NetworkFadeOutEntity(Game.PlayerPed.Handle, true, false);

            await BaseScript.Delay(50);

            Vector3 pos = player.Character.Position;
            Game.PlayerPed.Position = new Vector3(pos.X + 2f, pos.Y, pos.Z);

            await BaseScript.Delay(50);

            API.NetworkFadeInEntity(Game.PlayerPed.Handle, false);

            API.DoScreenFadeIn(200);
            await BaseScript.Delay(50);
        }

        static async Task Spectate(CitizenFX.Core.Player player)
        {
            if (!Player.PlayerInformation.IsStaff())
            {
                Debug.WriteLine("Don't know how you did that...");
                return;
            }

            if (player.ServerId == Game.Player.ServerId)
            {
                Debug.WriteLine("Cannot bring yourself to yourself");
                return;
            }

            API.DoScreenFadeOut(200);
            await BaseScript.Delay(200);

            int playerPedId = API.GetPlayerPed(player.Handle);

            if (_currentSpectate != null && _currentSpectate == player)
            {
                Client.TriggerEvent("curioisty:UI:IsSpectating", false);
                API.NetworkSetInSpectatorMode(false, playerPedId);

                Game.Player.IsInvincible = false;
                Game.PlayerPed.IsVisible = true;
                Game.PlayerPed.IsPositionFrozen = false;

                Game.PlayerPed.Detach();
                Game.PlayerPed.Position = _originalPosition;

                _originalPosition = Vector3.Zero;
                _currentSpectate = null;

                await BaseScript.Delay(50);

                API.DoScreenFadeIn(200);

                return;
            }

            API.ClearPlayerWantedLevel(Game.Player.Handle);

            if (_originalPosition == Vector3.Zero)
                _originalPosition = Game.PlayerPed.Position;

            _currentSpectate = player;

            Game.Player.IsInvincible = true;
            Game.PlayerPed.IsVisible = false;
            Game.PlayerPed.IsPositionFrozen = true;

            Vector3 newPos = _originalPosition;
            newPos.Z -= 50f;
            Game.PlayerPed.Position = newPos; // Fucking hide them
            
            API.NetworkSetInSpectatorMode(true, playerPedId);

            Client.TriggerEvent("curioisty:UI:IsSpectating", true);

            API.DoScreenFadeIn(200);
            await BaseScript.Delay(50);
        }

        public static void AddSubMenu(Menu menu, Menu submenu)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = "→→→" };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }
    }
}
