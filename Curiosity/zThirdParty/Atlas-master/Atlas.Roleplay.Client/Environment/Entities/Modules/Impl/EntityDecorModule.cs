using Atlas.Roleplay.Client.Diagnostics;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Entities.Modules.Impl
{
    public class EntityDecorModule : EntityModule
    {
        protected override void Begin(AtlasEntity entity, int id)
        {
        }

        public void Set(string property, object value)
        {
            if (value is int i)
            {
                if (!API.DecorExistOn(Id, property))
                {
                    API.DecorRegister(property, 3);
                }

                API.DecorSetInt(Id, property, i);
            }
            else if (value is float f)
            {
                if (!API.DecorExistOn(Id, property))
                {
                    API.DecorRegister(property, 1);
                }

                API.DecorSetFloat(Id, property, f);
            }
            else if (value is bool b)
            {
                if (!API.DecorExistOn(Id, property))
                {
                    API.DecorRegister(property, 2);
                }

                API.DecorSetBool(Id, property, b);
            }
            else
            {
                Logger.Info("[Decor] Could not set decor object due to it not being a supported type.");
            }
        }

        public int GetInteger(string property)
        {
            return API.DecorExistOn(Id, property) ? API.DecorGetInt(Id, property) : 0;
        }

        public float GetFloat(string property)
        {
            return API.DecorExistOn(Id, property) ? API.DecorGetFloat(Id, property) : 0f;
        }

        public bool GetBoolean(string property)
        {
            return API.DecorExistOn(Id, property) && API.DecorGetBool(Id, property);
        }
    }
}