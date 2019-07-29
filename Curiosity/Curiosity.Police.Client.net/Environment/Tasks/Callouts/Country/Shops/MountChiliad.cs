using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class MountChiliad
    {
        static string Name = "24/7, Mount Chiliad";
        static Vector3 Location = new Vector3(1730.532f, 6410.807f, 35.00065f);

        static Model SuspectModel = PedHash.Hippie01AFY;
        static Vector3 SuspectPosition = new Vector3(1735.893f, 6410.622f, 35.03723f);
        static float SuspectHeading = 66.84532f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1728.641f, 6416.965f, 35.03723f);
        static float ShopKeeperHeading = 246.1251f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
