﻿using CitizenFX.Core.Native;

namespace Curiosity.Callouts.Client.Utils
{
    class Decorators
    {
        public const string VEHICLE_STOLEN = "c::vehicle::stolen";
        public const string VEHICLE_FLEE = "c::vehicle::flee";

        public const string PED_FLEE = "c::vehicle::flee";
        public const string PED_SHOOT = "c::vehicle::shoot";
        public const string PED_ARREST = "c::vehicle::arrest";

        public static void Set(int handle, string property, object value)
        {
            if (value is int i)
            {
                if (!API.DecorExistOn(handle, property))
                {
                    API.DecorRegister(property, 3);
                }

                API.DecorSetInt(handle, property, i);
            }
            else if (value is float f)
            {
                if (!API.DecorExistOn(handle, property))
                {
                    API.DecorRegister(property, 1);
                }

                API.DecorSetFloat(handle, property, f);
            }
            else if (value is bool b)
            {
                if (!API.DecorExistOn(handle, property))
                {
                    API.DecorRegister(property, 2);
                }

                API.DecorSetBool(handle, property, b);
            }
            else
            {
                Logger.Log("[Decor] Could not set decor object due to it not being a supported type.");
            }
        }

        public static int GetInteger(int handle, string property)
        {
            return API.DecorExistOn(handle, property) ? API.DecorGetInt(handle, property) : 0;
        }

        public static float GetFloat(int handle, string property)
        {
            return API.DecorExistOn(handle, property) ? API.DecorGetFloat(handle, property) : 0f;
        }

        public static bool GetBoolean(int handle, string property)
        {
            return API.DecorExistOn(handle, property) && API.DecorGetBool(handle, property);
        }
    }
}
