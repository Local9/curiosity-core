using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class SandyShoresLiquorAce
    {
        static string Name = "Liquor ACE, Sandy Shores";
        static Vector3 Location = new Vector3(1394.598f, 3598.536f, 34.99088f);

        static Model SuspectModel = PedHash.Lost01GMY;
        static Vector3 SuspectPosition = new Vector3(1397.863f, 3605.385f, 34.98093f);
        static float SuspectHeading = 67.03603f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1393.069f, 3606.599f, 34.98093f);
        static float ShopKeeperHeading = 196.7299f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
