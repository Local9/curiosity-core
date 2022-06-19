namespace Curiosity.Core.Client.Managers.Milo.Casino
{
    class CasinoLuckyWheel
    {
        static PluginManager PluginManager => PluginManager.Instance;

        static int luckyWheelHandle;
        static Vector3 luckyWheelPosition = new Vector3(1111.052f, 229.8579f, -49.133f);

        static string luckyWheelAnimationDict;
        static string initialAnimation;

        static uint luckyWheelHash => (uint)GetHashKey("vw_prop_vw_luckywheel_02a");

        internal static void Init()
        {
            CreateProp();
            RequestGraphics();
        }

        internal static void Dispose()
        {
            SetModelAsNoLongerNeeded(luckyWheelHash);

            if (!string.IsNullOrEmpty(luckyWheelAnimationDict))
                RemoveAnimDict(luckyWheelAnimationDict);

            if (DoesEntityExist(luckyWheelHandle))
                DeleteEntity(ref luckyWheelHandle);
        }

        private static void RequestGraphics()
        {
            RequestStreamedTextureDict("CasinoUI_Lucky_Wheel", false);
        }

        private static async void CreateProp()
        {
            if (!DoesEntityExist(luckyWheelHandle))
            {
                Model model = (int)luckyWheelHash;
                model.Request(1000);

                int loadCheck = 0;

                while (!model.IsLoaded)
                {
                    if (loadCheck > 10)
                        break;

                    await BaseScript.Delay(10);
                    loadCheck++;
                }

                if (model.IsLoaded)
                {
                    luckyWheelHandle = CreateObjectNoOffset((uint)model.Hash, luckyWheelPosition.X, luckyWheelPosition.Y, luckyWheelPosition.Z, false, false, true);
                    SetEntitySomething(luckyWheelHandle, true);
                    SetEntityCanBeDamaged(luckyWheelHandle, false);
                }

                model.MarkAsNoLongerNeeded();
            }
        }
    }
}
