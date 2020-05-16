using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;

namespace Curiosity.Missions.Client.net.Extensions
{
    class Decorators
    {
        public const string DECOR_PED_NETWORKID = "c::ped::networkId";
        public const string DECOR_PED_INTERACTIVE = "c::ped::interactive";
        public const string DECOR_PED_MISSION = "c::ped::mission";


        public const string DECOR_PED_INFLUENCE_ALCAHOL = "c::ped::alcohol";
        public const string DECOR_PED_OFFENCE = "c::ped::offence";

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
                Log.Info("[Decor] Could not set decor object due to it not being a supported type.");
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
