﻿using CitizenFX.Core;

namespace Curiosity.Client.net.Classes.Menus.CharacterEditor.MPBodyMenu
{
    class ItemSelector : MenuItemHorSelector<int>
    {
        private CustomizeMenu.MPBodyMenu Parent;

        private int _value;
        public int Value
        {
            get { return _value; }
            private set { _value = MathUtil.Clamp(value, minState, maxState); }
        }

        public ItemSelector(CustomizeMenu.MPBodyMenu parent, string title, int max)
        {
            Parent = parent;
            Title = title;
            Type = MenuItemHorizontalSelectorType.Number;
            wrapAround = true;
            minState = 0;
            maxState = max;
            state = 0;
            Value = state;
            OnChange = SetNewAppearance;
        }

        private void SetNewAppearance(int value, MenuItemHorSelector<int> item)
        {
            Value = value;
            Parent.SetNewAppearance();
        }
    }
}
