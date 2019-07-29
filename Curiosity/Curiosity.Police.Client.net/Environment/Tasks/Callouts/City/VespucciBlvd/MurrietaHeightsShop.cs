using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.VespucciBlvd
{
    class MurrietaHeightsShop
    {
        static string Name = "Rob's Liquor on Vespucci Blvd";
        static Vector3 Location = new Vector3(1142.1f, -980.7694f, 46.20402f);

        static Model SuspectModel = PedHash.Dealer01SMY;
        static Vector3 SuspectPosition = new Vector3(1138.608f, -978.605f, 46.41584f);
        static float SuspectHeading = 130.9778f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1134.168f, -982.6425f, 46.41584f);
        static float ShopKeeperHeading = 278.8662f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
