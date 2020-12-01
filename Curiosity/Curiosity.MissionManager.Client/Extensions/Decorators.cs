using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using System;

namespace Curiosity.MissionManager.Client.Utils
{
    class Decorators
    {
        public const string PLAYER_DEBUG = "player::npc::debug";
        public const string PLAYER_DEBUG_UI = "player::ui::debug";
        public const string PLAYER_MENU = "player::npc::menu";
        public const string PLAYER_ASSISTING = "player::npc::assisting";
        public const string PLAYER_VEHICLE = "player::vehicle";
        public const string PLAYER_OWNER = "player::owner";

        public const string VEHICLE_STOLEN = "c::vehicle::stolen";
        public const string VEHICLE_FLEE = "c::vehicle::flee";
        public const string VEHICLE_MISSION = "c::vehicle::mission";
        public const string VEHICLE_SEARCH = "c::vehicle::search";
        public const string VEHICLE_TOW = "c::vehicle::tow";

        // LEGACY
        public const string VEHICLE_SPIKE_ALLOWED = "curiosity::police::vehicle::mission";

        public const string PED_FLEE = "c::ped::flee";
        public const string PED_SHOOT = "c::ped::shoot";
        public const string PED_FRIENDLY = "c::ped::friendly";
        public const string PED_ARREST = "c::ped::arrest";
        public const string PED_ARRESTED = "c::ped::arrested";
        public const string PED_ARRESTABLE = "c::ped::arrestable";
        public const string PED_SUSPECT = "c::ped::suspect";
        public const string PED_MISSION = "c::ped::mission";
        public const string PED_IMPORTANT = "c::ped::important";
        public const string PED_HOSTAGE = "c::ped::hostage";
        public const string PED_RELEASED = "c::ped::released";
        public const string PED_HANDCUFFED = "c::ped::handcuffed";
        public const string PED_DIALOGUE = "c::ped::dialogue";
        public const string PED_SETUP = "c::ped::setup";

        // ARREST REASONS
        public const string PED_ARREST_INFLUENCE_DRUNK = "c::ped::arrest::influence::drunk";
        public const string PED_ARREST_INFLUENCE_COCAINE = "c::ped::arrest::influence::cocaine";
        public const string PED_ARREST_INFLUENCE_HEROIN = "c::ped::arrest::influence::heroin";
        public const string PED_ARREST_INFLUENCE_WEED = "c::ped::arrest::influence::weed";

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
                Logger.Debug("[Decor] Could not set decor object due to it not being a supported type.");
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

        internal static bool GetBoolean(int handle, object pED_SETUP)
        {
            throw new NotImplementedException();
        }
    }
}
