using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.Morningwood
{
    class ProsperityStreetShop
    {
        static string Name = "Rob's Liquor on Prosperity St";
        static Vector3 Location = new Vector3(-1491.354f, -384.2097f, 40.08645f);

        static Model SuspectModel = PedHash.BikerChic;
        static Vector3 SuspectPosition = new Vector3(-1487.207f, -382.805f, 40.16343f);
        static float SuspectHeading = 351.4111f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-1486.434f, -377.7036f, 40.16343f);
        static float ShopKeeperHeading = 143.9395f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
