using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.Shops
{
    class DavisAve
    {
        static string Name = "LTD Garage, Davis Ave";
        static Vector3 Location = new Vector3(-53.7861f, -1757.661f, 29.43897f);

        static Model SuspectModel = PedHash.Dealer01SMY;
        static Vector3 SuspectPosition = new Vector3(-47.62278f, -1753.076f, 29.42101f);
        static float SuspectHeading = 190.1924f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-46.76466f, -1758.037f, 29.42101f);
        static float ShopKeeperHeading = 42.98832f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
