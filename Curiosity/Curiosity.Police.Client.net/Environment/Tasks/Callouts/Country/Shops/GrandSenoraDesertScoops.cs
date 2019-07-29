using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class GrandSenoraDesertScoops
    {
        static string Name = "Scoops Liquor Barn";
        static Vector3 Location = new Vector3(1166.458f, 2702.856f, 38.17914f);

        static Model SuspectModel = PedHash.Hillbilly01AMM;
        static Vector3 SuspectPosition = new Vector3(1168.959f, 2706.064f, 38.1577f);
        static float SuspectHeading = 38.5031f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1166.744f, 2710.851f, 38.1577f);
        static float ShopKeeperHeading = 179.802f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
