using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.Shops
{
    class SanAndreasAve
    {
        static string Name = "Rob's Liquor on San Andreas Ave";
        static Vector3 Location = new Vector3(-1226.846f, -901.4744f, 12.28888f);

        static Model SuspectModel = PedHash.Eastsa01AMM;
        static Vector3 SuspectPosition = new Vector3(-1224.359f, -905.7396f, 12.32636f);
        static float SuspectHeading = 233.6525f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-1222.146f, -908.513f, 12.32636f);
        static float ShopKeeperHeading = 26.50367f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
