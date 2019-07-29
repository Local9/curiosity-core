using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Rural.Shops
{
    class RichmanGlen
    {
        static string Name = "24/7 on Richman Glen";
        static Vector3 Location = new Vector3(-1819.346f, 786.3083f, 137.9569f);

        static Model SuspectModel = PedHash.Stwhi02AMY;
        static Vector3 SuspectPosition = new Vector3(-1826.983f, 790.7717f, 138.2379f);
        static float SuspectHeading = 321.5045f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-1820.645f, 794.8521f, 138.0906f);
        static float ShopKeeperHeading = 145.9614f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
