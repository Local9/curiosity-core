using Curiosity.Framework.Shared.Enums;

namespace Curiosity.Framework.Client.Extensions
{
    internal class DecoratorExtension
    {
        public static void Set(int handle, string property, object value)
        {
            int decorType = -1;

            if (value is int i)
            {
                decorType = (int)eDecorType.TYPE_INT;

                if (!DecorIsRegisteredAsType(property, decorType))
                    DecorRegister(property, decorType);

                if (!DecorExistOn(handle, property))
                    DecorRegister(property, decorType);

                DecorSetInt(handle, property, i);
            }
            else if (value is float f)
            {
                decorType = (int)eDecorType.TYPE_FLOAT;

                if (!DecorIsRegisteredAsType(property, decorType))
                    DecorRegister(property, decorType);

                if (!DecorExistOn(handle, property))
                    DecorRegister(property, decorType);

                DecorSetFloat(handle, property, f);
            }
            else if (value is bool b)
            {
                decorType = (int)eDecorType.TYPE_BOOL;

                if (!DecorIsRegisteredAsType(property, decorType))
                    DecorRegister(property, decorType);

                if (!DecorExistOn(handle, property))
                    DecorRegister(property, decorType);

                DecorSetBool(handle, property, b);
            }
            else
            {
                Logger.Info("[Decor] Could not set decor object due to it not being a supported type.");
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
