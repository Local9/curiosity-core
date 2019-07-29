using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class SandyShoresTwentyFour
    {
        static string Name = "24/7, Sandy Shores";
        static Vector3 Location = new Vector3(1965.68f, 3739.752f, 32.32018f);

        static Model SuspectModel = PedHash.Hippie01;
        static Vector3 SuspectPosition = new Vector3(1961.589f, 3744.87f, 32.34375f);
        static float SuspectHeading = 203.0614f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1959.762f, 3740.436f, 32.34375f);
        static float ShopKeeperHeading = 298.2224f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
