using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Rural.Shops
{
    class Chumash
    {
        static string Name = "24/7 on Chumash";
        static Vector3 Location = new Vector3(-3238.816f, 1004.304f, 12.45766f);

        static Model SuspectModel = PedHash.StrPunk01GMY;
        static Vector3 SuspectPosition = new Vector3(-3245.716f, 1004.078f, 12.83071f);
        static float SuspectHeading = 207.4452f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-3244.055f, 1000.066f, 12.83071f);
        static float ShopKeeperHeading = 349.1961f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
