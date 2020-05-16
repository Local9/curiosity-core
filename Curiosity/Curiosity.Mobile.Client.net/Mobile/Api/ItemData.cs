using CitizenFX.Core.Native;
using System;

namespace Curiosity.Mobile.Client.net.Mobile.Api
{
    class ItemData
    {
        // Internal data.
        protected dynamic value;
        protected Func<bool> action;

        public ItemData(dynamic value)
        {
            this.value = value;
            if (value is int)
            {
                action = PushInt;
            }
            else if (value is float)
            {
                action = PushFloat;
            }
            else if (value is string)
            {
                action = PushString;
            }
        }

        public void Push()
        {
            action.Invoke();
        }

        // Push functions.
        public bool PushInt()
        {
            API.PushScaleformMovieFunctionParameterInt(value);
            return true;
        }
        public bool PushFloat()
        {
            API.PushScaleformMovieFunctionParameterFloat(value);
            return true;
        }
        public bool PushString()
        {
            API.PushScaleformMovieFunctionParameterString(value);
            return true;
        }
    }

}
