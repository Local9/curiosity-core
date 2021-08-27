﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace NativeUI
{
    [Flags]
    public enum PadCheck
    {
        Any = 0,
        Keyboard = 1,
        Controller = 2
    }

    public delegate void OnInstructionControlSelected(Control control);
    public class InstructionalButton
    {
        public event OnInstructionControlSelected OnControlSelected;
        public string Text { get; set; }
        public bool IsUsingController => !API.IsUsingKeyboard(2);

        public UIMenuItem ItemBind { get; private set; }

        public Control GamepadButton { get; private set; }
        public Control KeyboardButton { get; private set; }
        public List<Control> GamepadButtons { get; private set; }
        public List<Control> KeyboardButtons { get; private set; }
        public PadCheck PadCheck { get; private set; }

        /// <summary>
        /// Add a dynamic button to the instructional buttons array.
        /// Changes whether the controller is being used and changes depending on keybinds.
        /// </summary>
        /// <param name="control">Control that gets converted into a button.</param>
        /// <param name="text">Help text that goes with the button.</param>
        /// <param name="padFilter">Filter to show only with GamePad or Keyoboard (default both).</param>
        public InstructionalButton(Control control, string text, PadCheck padFilter = PadCheck.Any)
        {
            if (padFilter == PadCheck.Controller)
                GamepadButton = control;
            else if (padFilter == PadCheck.Keyboard)
                KeyboardButton = control;
            else if (padFilter == PadCheck.Any)
            {
                GamepadButton = control;
                KeyboardButton = control;
            }
            Text = text;
            PadCheck = padFilter;
        }

        /// <summary>
        /// Add a dynamic button to the instructional buttons array.
        /// </summary>
        /// <param name="controls">List of controls that get converted into a single button</param>
        /// <param name="text">Help text that goes with the button.</param>
        /// <param name="padFilter">Filter to show only with GamePad or Keyoboard (default both).</param>
        public InstructionalButton(List<Control> controls, string text, PadCheck padFilter = PadCheck.Any)
        {
            if (padFilter == PadCheck.Controller)
                GamepadButtons = controls;
            else if (padFilter == PadCheck.Keyboard)
                KeyboardButtons = controls;
            else if (padFilter == PadCheck.Any)
            {
                GamepadButtons = controls;
                KeyboardButtons = controls;
            }
            Text = text;
            PadCheck = padFilter;
        }

        /// <summary>
        /// Add a dynamic button to the instructional buttons array.
        /// </summary>
        /// <param name="gamepadControl">The control that will be shown if using the GamePad</param>
        /// <param name="keyboardControl">The control that will be shown if using the Keyboard</param>
        /// <param name="text">Help text that goes with the button.</param>
        public InstructionalButton(Control gamepadControl, Control keyboardControl, string text)
        {
            Text = text;
            GamepadButton = gamepadControl;
            KeyboardButton = keyboardControl;
            PadCheck = PadCheck.Any;
        }

        /// <summary>
        /// Add a dynamic button to the instructional buttons array.
        /// </summary>
        /// <param name="gamepadControl">The list of controls that will be shown if using the GamePad</param>
        /// <param name="keyboardControl">The list of controls that will be shown if using the Keyboard</param>
        /// <param name="text">Help text that goes with the button.</param>
        public InstructionalButton(List<Control> gamepadControls, List<Control> keyboardControls, string text)
        {
            Text = text;
            GamepadButtons = gamepadControls;
            KeyboardButtons = keyboardControls;
            PadCheck = PadCheck.Any;
        }

        /// <summary>
        /// Bind this button to an item, so it's only shown when that item is selected.
        /// </summary>
        /// <param name="item">Item to bind to.</param>
        public void BindToItem(UIMenuItem item)
        {
            ItemBind = item;
        }

        public string GetButtonId()
        {
            if (KeyboardButtons != null || GamepadButtons != null)
            {
                string retVal = "";
                if (IsUsingController)
                {
                    for (int i = GamepadButtons.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                            retVal += API.GetControlInstructionalButton(2, (int)GamepadButtons[i], 1);
                        else
                            retVal += API.GetControlInstructionalButton(2, (int)GamepadButtons[i], 1) + "%";
                    }
                }
                else
                {
                    for (int i = KeyboardButtons.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                            retVal += API.GetControlInstructionalButton(2, (int)KeyboardButtons[i], 1);
                        else
                            retVal += API.GetControlInstructionalButton(2, (int)KeyboardButtons[i], 1) + "%";
                    }
                }
                return retVal;
            }
            return IsUsingController ? API.GetControlInstructionalButton(2, (int)GamepadButton, 1) : API.GetControlInstructionalButton(2, (int)KeyboardButton, 1);
        }

        public void InvokeEvent(Control control)
        {
            OnControlSelected?.Invoke(control);
        }
    }

    public class InstructionalButtonsScaleform
    {
        private Scaleform _sc;
        private bool _useMouseButtons;
        private bool _enabled = false;
        internal bool _isUsingKeyboard;
        internal bool _changed = true;
        internal int savingTimer = 0;
        private bool _isSaving = false;

        /// <summary>
        /// Show or Hide the Instructional Buttons
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (!value)
                {
                    _sc.CallFunction("CLEAR_ALL");
                    _sc.CallFunction("CLEAR_RENDER");
                }
                _enabled = value;
                _changed = true;
            }
        }

        /// <summary>
        /// If you set this to true the user will see the mouse cursor on screen
        /// </summary>
        public bool UseMouseButtons
        {
            get => _useMouseButtons;
            set => _useMouseButtons = value;
        }

        /// <summary>
        /// Returns true if the Saving button is showing
        /// </summary>
        public bool IsSaving
        {
            get => _isSaving;
            set => _isSaving = value;
        }

        public List<InstructionalButton> ControlButtons { get; private set; }

        internal async void Load()
        {
            if (_sc != null) return;
            _sc = new Scaleform("INSTRUCTIONAL_BUTTONS");
            int timeout = 1000;
            DateTime start = DateTime.Now;
            while (!API.HasScaleformMovieLoaded(_sc.Handle) && DateTime.Now.Subtract(start).TotalMilliseconds < timeout) await BaseScript.Delay(0);
        }

		/// <summary>
		/// Set the list of buttons at once (can be edited after).
		/// </summary>
		/// <param name="buttons">List of <see cref="InstructionalButton"/> to show.</param>
		public void SetInstructionalButtons(List<InstructionalButton> buttons)
        {
            ControlButtons = buttons;
            _changed = true;
        }

		/// <summary>
		/// Adds an <see cref="InstructionalButton"/> to the List (on the left)
		/// </summary>
		/// <param name="button"></param>
		public void AddInstructionalButton(InstructionalButton button)
        {
            ControlButtons.Add(button);
            _changed = true;
        }

        /// <summary>
        /// Removes an <see cref="InstructionalButton"/>
        /// </summary>
        /// <param name="button">The <see cref="InstructionalButton"/> to remove.</param>
        public void RemoveInstructionalButton(InstructionalButton button)
        {
            ControlButtons.Remove(button);
            _changed = true;
        }

        /// <summary>
        /// Removes an <see cref="InstructionalButton"/>
        /// </summary>
        /// <param name="button">The index to remove.</param>
        public void RemoveInstructionalButton(int button)
        {
            ControlButtons.RemoveAt(button);
            _changed = true;
        }

        /// <summary>
        /// Shows the Saving / Loading Button
        /// </summary>
        /// <param name="spinnerType">The type of Spinner to show</param>
        /// <param name="text">The text of the Button</param>
        /// <param name="time">Duration of the Button</param>
        public async void AddSavingText(LoadingSpinnerType spinnerType, string text, int time)
        {
            _isSaving = true;
            _changed = true;
            savingTimer = Game.GameTime;
            Screen.LoadingPrompt.Show(text, spinnerType);
            while (Game.GameTime - savingTimer < time) await BaseScript.Delay(100);
            Screen.LoadingPrompt.Hide();
            _isSaving = false;
        }

        internal void UpdateButtons()
        {
            if (!_changed) return;
            _sc.CallFunction("TOGGLE_MOUSE_BUTTONS", _useMouseButtons);
            int count = 0;

            foreach (InstructionalButton button in ControlButtons)
            {
                if (button.IsUsingController)
                {
                    if (button.PadCheck == PadCheck.Keyboard) continue;
                    if (PopupWarningThread.Warning.IsShowing)
                        _sc.CallFunction("SET_DATA_SLOT", count, button.GetButtonId(), button.Text, 0, -1);
                    else
                        _sc.CallFunction("SET_DATA_SLOT", count, button.GetButtonId(), button.Text);
                }
                else
                {
                    if (button.PadCheck == PadCheck.Controller) continue;
                    if (_useMouseButtons)
                        _sc.CallFunction("SET_DATA_SLOT", count, button.GetButtonId(), button.Text, 1, (int)button.KeyboardButton);
                    else
                    {
                        if (PopupWarningThread.Warning.IsShowing)
                            _sc.CallFunction("SET_DATA_SLOT", count, button.GetButtonId(), button.Text, 0, -1);
                        else
                            _sc.CallFunction("SET_DATA_SLOT", count, button.GetButtonId(), button.Text);

                    }

                }
                count++;
            }
            _sc.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            _changed = false;
        }

        /// <summary>
        /// Draws the InstructionalButtons
        /// </summary>
        public void Draw()
        {
            _sc.Render2D();
        }

        /// <summary>
        /// Draws the InstructionalButtons overriding position
        /// </summary>
        /// <param name="Position">the values for variation must be in 0.000 (One thousandth) decimal places for precision</param>
        public void Draw(PointF Position)
		{
            API.DrawScaleformMovie(_sc.Handle, 0.5f - Position.X, 0.5f - Position.Y, 1f, 1f, 255, 255, 255, 255, 0);
        }

        internal void HandleScaleform()
        {
            if (_sc == null || !_enabled) return;
            if ((ControlButtons == null || ControlButtons.Count == 0) && !_isSaving) return;
            if (API.IsUsingKeyboard(2))
            {
                if (!_isUsingKeyboard)
                {
                    _isUsingKeyboard = true;
                    _changed = true;
                }
            }
            else
            {
                if (_isUsingKeyboard)
                {
                    _isUsingKeyboard = false;
                    _changed = true;
                }
            }
            UpdateButtons();

            if (!PopupWarningThread.Warning.IsShowing) Draw();

            foreach (InstructionalButton button in ControlButtons)
            {
                if (IsControlJustPressed(button.GamepadButton, button.PadCheck) || (button.GamepadButtons != null && button.GamepadButtons.Any(x => IsControlJustPressed(x, button.PadCheck))))
                    button.InvokeEvent(button.GamepadButton);
                else if (IsControlJustPressed(button.KeyboardButton, button.PadCheck) || (button.KeyboardButtons != null && button.KeyboardButtons.Any(x => IsControlJustPressed(x, button.PadCheck))))
                    button.InvokeEvent(button.KeyboardButton);
            }
            if (_useMouseButtons)
                Screen.Hud.ShowCursorThisFrame();
            Screen.Hud.HideComponentThisFrame(HudComponent.VehicleName);
            Screen.Hud.HideComponentThisFrame(HudComponent.AreaName);
            Screen.Hud.HideComponentThisFrame(HudComponent.StreetName);
        }

        public static bool IsControlJustPressed(Control control, PadCheck keyboardOnly = PadCheck.Any) => Game.IsControlJustPressed(0, control) && (keyboardOnly == PadCheck.Keyboard ? !API.IsUsingKeyboard(2) : keyboardOnly == PadCheck.Controller ? API.IsUsingKeyboard(2) : true);
	}

    public class InstructionalButtonsHandler : BaseScript
    {
        public static InstructionalButtonsScaleform InstructionalButtons;
        public InstructionalButtonsHandler()
        {
            InstructionalButtons = new InstructionalButtonsScaleform();
            InstructionalButtons.Load();
            Tick += InstructionalButtons_Tick;
        }

        private async Task InstructionalButtons_Tick()
        {
            InstructionalButtons.HandleScaleform();
            await Task.FromResult(0);
        }
    }

}