﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using NativeUI.PauseMenu;
using Control = CitizenFX.Core.Control;

namespace NativeUI
{

    public delegate void MenuStateChangeEvent(UIMenu oldMenu, UIMenu newMenu, MenuState state);

    /// <summary>
    /// Helper class that handles all of your Menus. After instatiating it, you will have to add your menu by using the Add method.
    /// </summary>
    public class MenuPool
    {
        private static Ped _ped;
        internal static Ped PlayerPed { get
			{
                int handle = API.PlayerPedId();

                if (_ped is null || handle != _ped.Handle)
                {
                    _ped = new Ped(handle);
                }

                return _ped;
            }
        }

        public bool MouseEdgeEnabled { set { _menuList.ForEach(m => m.MouseEdgeEnabled = value); } }

        public bool ControlDisablingEnabled { set { _menuList.ForEach(m => m.ControlDisablingEnabled = value); } }

        public bool ResetCursorOnOpen { set { _menuList.ForEach(m => m.ResetCursorOnOpen = value); } }

        public bool FormatDescriptions { set { _menuList.ForEach(m => m.FormatDescriptions = value); } }

        public string AUDIO_LIBRARY { set { _menuList.ForEach(m => m.AUDIO_LIBRARY = value); } }

        public string AUDIO_UPDOWN { set { _menuList.ForEach(m => m.AUDIO_UPDOWN = value); } }

        public string AUDIO_SELECT { set { _menuList.ForEach(m => m.AUDIO_SELECT = value); } }

        public string AUDIO_BACK { set { _menuList.ForEach(m => m.AUDIO_BACK = value); } }

        public string AUDIO_ERROR { set { _menuList.ForEach(m => m.AUDIO_ERROR = value); } }

        public int WidthOffset { set { _menuList.ForEach(m => m.SetMenuWidthOffset(value)); } }

        public string CounterPretext { set { _menuList.ForEach(m => m.CounterPretext = value); } }

        public bool DisableInstructionalButtons { set { _menuList.ForEach(m => m.DisableInstructionalButtons(value)); } }

		public bool BannerInheritance = true;

		public bool OffsetInheritance = true;

        /// <summary>
		/// Called when user either opens or closes the main menu, clicks on a binded button, goes back to a parent menu.
        /// </summary>
        public event MenuStateChangeEvent OnMenuStateChanged;

        private readonly List<UIMenu> _menuList = new List<UIMenu>();
        private readonly List<TabView> _pauseMenus = new List<TabView>();

        /// <summary>
        /// Add your menu to the menu pool.
        /// </summary>
        /// <param name="menu"></param>
        public void Add(UIMenu menu)
        {
            _menuList.Add(menu);
            menu._poolcontainer = this;
        }

        /// <summary>
        /// Add your pause menu to the menu pool.
        /// </summary>
        /// <param name="menu"></param>
        public void AddPauseMenu(TabView menu)
        {
            _pauseMenus.Add(menu);
        }

        /// <summary>
        /// Create and add a submenu to the menu pool.
        /// Adds an item with the given text to the menu, creates a corresponding submenu, and binds the submenu to the item.
        /// The submenu inherits its title from the menu, and its subtitle from the item text.
        /// </summary>
        /// <param name="menu">The parent menu to which the submenu must be added.</param>
        /// <param name="text">The name of the submenu.</param>
        /// <returns>The newly created submenu.</returns>
        public UIMenu AddSubMenu(UIMenu menu, string text)
        {
			PointF Offset = PointF.Empty;
			if (OffsetInheritance)
				Offset = menu.Offset;
			return AddSubMenu(menu, text, "", Offset);
		}

		/// <summary>
		/// Create and add a submenu to the menu pool with a custom offset.
		/// Adds an item with the given text to the menu, creates a corresponding submenu, and binds the submenu to the item.
		/// The submenu inherits its title from the menu, and its subtitle from the item text.
		/// </summary>
		/// <param name="menu">The parent menu to which the submenu must be added.</param>
		/// <param name="text">The name of the submenu</param>
		/// <param name="offset">The offset of the menu</param>
		/// <returns>The newly created submenu.</returns>
		public UIMenu AddSubMenu(UIMenu menu, string text, PointF offset)
		{
			return AddSubMenu(menu, text, "", Point.Empty);
		}

		/// <summary>
		/// Create and add a submenu to the menu pool.
		/// Adds an item with the given text and description to the menu, creates a corresponding submenu, and binds the submenu to the item.
		/// The submenu inherits its title from the menu, and its subtitle from the item text.
		/// </summary>
		/// <param name="menu">The parent menu to which the submenu must be added.</param>
		/// <param name="text">The name of the submenu.</param>
		/// <param name="description">The name of the submenu.</param>
		/// <returns>The newly created submenu.</returns>
		public UIMenu AddSubMenu(UIMenu menu, string text, string description)
        {
			PointF Offset = PointF.Empty;
			if (OffsetInheritance)
				Offset = menu.Offset;
			return AddSubMenu(menu, text, description, Offset);
		}

		/// <summary>
		/// Create and add a submenu to the menu pool.
		/// Adds an item with the given text and description to the menu, creates a corresponding submenu, and binds the submenu to the item.
		/// The submenu inherits its title from the menu, and its subtitle from the item text.
		/// </summary>
		/// <param name="menu">The parent menu to which the submenu must be added.</param>
		/// <param name="text">The name of the submenu.</param>
		/// <param name="description">The name of the submenu.</param>
		/// <returns>The newly created submenu.</returns>
		public UIMenu AddSubMenu(UIMenu menu, string text, string description, PointF offset)
		{
			UIMenuItem item = new UIMenuItem(text, description);
			menu.AddItem(item);
			UIMenu submenu = new UIMenu(menu.Title.Caption, text, offset);
			if (BannerInheritance && menu.BannerTexture != null)
				submenu.SetBannerType(menu.BannerTexture);
			else if (BannerInheritance && menu.BannerRectangle != null)
				submenu.SetBannerType(menu.BannerRectangle);
			else if (BannerInheritance && menu.BannerSprite != null)
				submenu.SetBannerType(menu.BannerSprite);
            submenu.MouseControlsEnabled = menu.MouseControlsEnabled;
            submenu.MouseEdgeEnabled = menu.MouseEdgeEnabled;
            Add(submenu);
			menu.BindMenuToItem(submenu, item);
            menu._poolcontainer = this;
            return submenu;

		}

		/// <summary>
		/// Refresh index of every menu in the pool.
		/// Use this after you have finished constructing the entire menu pool.
		/// </summary>
		public void RefreshIndex()
        {
            foreach (UIMenu menu in _menuList) menu.RefreshIndex();
        }

        /// <summary>
        /// Returns all of your menus.
        /// </summary>
        /// <returns></returns>
        public List<UIMenu> ToList()
        {
            return _menuList;
        }

        /// <summary>
        /// Processes all of your visible menus' controls.
        /// </summary>
        public void ProcessControl()
        {
            /*foreach (var menu in _menuList.Where(menu => menu.Visible)) // foreach works slower with List. Also why make a new enumerable every tick
            {
                menu.ProcessControl();
            }*/

            int count = _menuList.Count; // Cache count
            for (int i = 0; i < count; i++)
            {
                if (_menuList[i].Visible)
                    _menuList[i].ProcessControl();
            }
        }


        /// <summary>
        /// Processes all of your visible menus' keys.
        /// </summary>
        /// <param name="key"></param>
        public void ProcessKey(Keys key)
        {
            /*foreach (var menu in _menuList.Where(menu => menu.Visible))
            {
                menu.ProcessKey(key);
            }*/

            int count = _menuList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_menuList[i].Visible)
                    _menuList[i].ProcessKey(key);
            }
        }


        /// <summary>
        /// Processes all of your visible menus' mouses.
        /// </summary>
        public void ProcessMouse()
        {
            /*foreach (var menu in _menuList.Where(menu => menu.Visible))
            {
                menu.ProcessMouse();
            }*/

            int count = _menuList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_menuList[i].Visible)
                    _menuList[i].ProcessMouse();
            }
        }


        /// <summary>
        /// Draws all visible menus.
        /// </summary>
        public void Draw()
        {
            /*foreach (var menu in _menuList.Where(menu => menu.Visible))
            {
                menu.Draw();
            }*/

            int count = _menuList.Count;
            for (int i = 0; i < count; i++)
            {
                if (_menuList[i].Visible)
                    _menuList[i].Draw();
            }
        }


        /// <summary>
        /// Checks if any menu is currently visible.
        /// </summary>
        /// <returns>true if at least one menu is visible, false if not.</returns>
        public bool IsAnyMenuOpen()
        {
            return _menuList.Any(menu => menu.Visible) || _pauseMenus.Any(x => x.Visible);
        }


        /// <summary>
        /// Process all of your menus' functions. Call this in a tick event.
        /// </summary>
        public void ProcessMenus()
        {
            ProcessControl();
            if (_pauseMenus.Count > 0)
                _pauseMenus.ForEach(x => { x.ProcessControls(); x.Draw(); });
            ProcessMouse();
            Draw();
        }


        /// <summary>
        /// Closes all of your menus.
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var menu in _menuList.Where(menu => menu.Visible))
            {
                menu.Visible = false;
            }
        }

        public void SetBannerType(Sprite bannerType)
        {
            _menuList.ForEach(m => m.SetBannerType(bannerType));
        }

        public void SetBannerType(UIResRectangle bannerType)
        {
            _menuList.ForEach(m => m.SetBannerType(bannerType));
        }

        public void SetBannerType(string bannerPath)
        {
            _menuList.ForEach(m => m.SetBannerType(bannerPath));
        }

        public void SetKey(UIMenu.MenuControls menuControl, Control control)
        {
            _menuList.ForEach(m => m.SetKey(menuControl, control));
        }

        public void SetKey(UIMenu.MenuControls menuControl, Control control, int controllerIndex)
        {
            _menuList.ForEach(m => m.SetKey(menuControl, control, controllerIndex));
        }

        public void SetKey(UIMenu.MenuControls menuControl, Keys control)
        {
            _menuList.ForEach(m => m.SetKey(menuControl, control));
        }

        public void ResetKey(UIMenu.MenuControls menuControl)
        {
            _menuList.ForEach(m => m.ResetKey(menuControl));
        }

        public void MenuChangeEv(UIMenu oldmenu, UIMenu newmenu, MenuState state)
        {
            OnMenuStateChanged?.Invoke(oldmenu, newmenu, state);
        }
    }
}
