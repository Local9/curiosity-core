using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class Grapeseed
    {
        static string Name = "LTD Grage, Grapeseed";
        static Vector3 Location = new Vector3(1698.641f, 4929.917f, 42.0781f);

        static Model SuspectModel = PedHash.Farmer01AMM;
        static Vector3 SuspectPosition = new Vector3(1703.9f, 4927.225f, 42.06367f);
        static float SuspectHeading = 130.5826f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1698.638f, 4922.441f, 42.06367f);
        static float ShopKeeperHeading = 354.2891f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
