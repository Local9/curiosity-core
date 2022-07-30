using CitizenFX.Core.UI;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CreatorMenus
    {
        // menu pool
        public static MenuPool _MenuPool;
        bool menuOpen = true;
        // Menus
        private UIMenu menuMain;
        private UIMenu menuCharacterHeritage;
        // private UIMenu menuPlayerLifeStyle; // 
        private UIMenu menuCharacterFeatures; // eyes, hair?
        private UIMenu menuCharacterAppearance; // clothes
        private UIMenu menuCharacterTattoos; // Tattoos
        private UIMenu menuCharacterCustomisation; // clothing
        private UIMenu menuCharacterProps; // clothing
        private UIMenu menuCharacterStats; // stats
        private UIMenuItem itemSave;
        // Cameras
        public static RotatablePosition[] CameraViews = CharacterExtensions.CameraViews;

        // items
        private List<dynamic> Genders = new List<dynamic> { $"{Gender.Male}", $"{Gender.Female}" };
        private UIMenuListItem mLstGender;

        // buttons
        public static InstructionalButton btnRotateLeft = new InstructionalButton(Control.Cover, "Spin Left");
        public static InstructionalButton btnRotateRight = new InstructionalButton(Control.Pickup, "Spin Right");
        public static InstructionalButton btnRandom = new InstructionalButton(Control.Jump, "Random");

        // Classes
        private CharacterHeritage _CharacterHeritage = new CharacterHeritage();
        private CharacterFeatures _CharacterFeatures = new CharacterFeatures();
        private CharacterCustomisation _CharacterCustomisation = new CharacterCustomisation();
        private CharacterAppearance _CharacterAppearance = new CharacterAppearance();
        private CharacterTattoos _CharacterTattoos = new CharacterTattoos();
        private CharacterProps _CharacterProps = new CharacterProps();

        internal async void CreateMenu(bool open = false, bool canChangeParents = true)
        {
            // TICKS & SETUP
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;

            //UIMenu
            menuMain = new UIMenu(Game.Player.Name, "Player Creator");
            _MenuPool.Add(menuMain);

            menuMain.Clear();

            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnItemSelect += MenuMain_OnItemSelect;

            if (open)
            {
                await Cache.PlayerPed.FadeOut();
                Cache.Player.Character.MarkedAsRegistered = false;

                EventSystem.GetModule().Send("character:routing:creator");

                CharacterManager.GetModule().Load(Cache.Player);
            }

            // items

            mLstGender = new UIMenuListItem("Gender", Genders, Cache.Character.Gender);
            menuMain.AddItem(mLstGender);

            if (canChangeParents)
            {
                menuCharacterHeritage = _MenuPool.AddSubMenu(menuMain, "Heritage");
                _CharacterHeritage.CreateMenu(menuCharacterHeritage);
            }

            menuCharacterFeatures = _MenuPool.AddSubMenu(menuMain, "Features");
            _CharacterFeatures.CreateMenu(menuCharacterFeatures);

            menuCharacterAppearance = _MenuPool.AddSubMenu(menuMain, "Appearance");
            _CharacterAppearance.CreateMenu(menuCharacterAppearance);

            menuCharacterTattoos = _MenuPool.AddSubMenu(menuMain, "Tattoos");
            _CharacterTattoos.CreateMenu(menuCharacterTattoos);

            menuCharacterCustomisation = _MenuPool.AddSubMenu(menuMain, "Apparel");
            _CharacterCustomisation.CreateMenu(menuCharacterCustomisation);

            menuCharacterProps = _MenuPool.AddSubMenu(menuMain, "Props");
            _CharacterProps.CreateMenu(menuCharacterProps);

            itemSave = new UIMenuItem("Save", "Save your character and enter the world.");
            menuMain.AddItem(itemSave);

            // buttons
            menuMain.InstructionalButtons.Add(btnRotateLeft);
            menuMain.InstructionalButtons.Add(btnRotateRight);

            menuMain.ResetKey(UIMenu.MenuControls.Back);

            _MenuPool.RefreshIndex();
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            try
            {
                if (selectedItem == itemSave)
                {
                    Cache.Character.MarkedAsRegistered = true;
                    Cache.Player.Character = Cache.Character;
                    await Cache.Character.Save();

                    EventSystem.GetModule().Send("character:routing:base");

                    DestroyMenus();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"MenuMain_OnItemSelect");
            }
        }

        private async void MenuMain_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mLstGender) // Player Gender
            {
                Model playerModel = PedHash.FreemodeMale01;
                if (newIndex == 1)
                {
                    playerModel = PedHash.FreemodeFemale01;
                }
                await playerModel.Request(10000);
                Screen.Fading.FadeOut(500);
                while (Screen.Fading.IsFadingOut) await BaseScript.Delay(10);

                await Game.Player.ChangeModel(playerModel);

                if (Cache.Character.Heritage == null)
                {
                    Cache.Character.Heritage = new Systems.Library.Models.CharacterHeritage();
                }

                Cache.Character.Gender = newIndex;

                _CharacterHeritage.UpdatePedBlendData();

                await BaseScript.Delay(500);

                playerModel.MarkAsNoLongerNeeded();

                Screen.Fading.FadeIn(1000);
                while (Screen.Fading.IsFadingIn) await BaseScript.Delay(10);
                return;
            }
        }

        private void DestroyMenus()
        {
            try
            {
                PluginManager.Instance.DetachTickHandler(OnMenuCreate);
                PluginManager.Instance.DetachTickHandler(OnPlayerControls);

                menuMain.InstructionalButtons.Clear();

                _MenuPool.CloseAllMenus();

                PluginManager.MenuPool.MouseEdgeEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Destroy Menus");
            }
        }

        private async Task OnPlayerControls()
        {
            float turnSpeed = 1f;

            if (Game.IsControlPressed(0, Control.Sprint))
                turnSpeed += 1f;

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += turnSpeed;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= turnSpeed;
            }
        }

        private async Task OnMenuCreate()
        {
            try
            {
                _MenuPool.ProcessMenus();
                _MenuPool.ProcessMouse();
            }
            catch (KeyNotFoundException ex)
            {

            }
            catch (IndexOutOfRangeException ex)
            {

            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"OnMenuCreate");
            }
        }

        internal void AddSubMenu(UIMenu menu, string title)
        {
            _MenuPool.AddSubMenu(menu, title);
        }

        internal void OpenMenu()
        {
            PluginManager.Instance.DiscordRichPresence.Status = "Character Creator";
            PluginManager.Instance.DiscordRichPresence.Commit();

            PluginManager.Instance.AttachTickHandler(OnPlayerControls);
            PluginManager.Instance.AttachTickHandler(OnMenuCreate);

            PluginManager.MenuPool.MouseEdgeEnabled = false;

            menuMain.Visible = true;
        }
    }
}
