using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.Shops
{
    class LittleSeoul
    {
        static string Name = "LTD on Palomino Ave & Ginger St";
        static Vector3 Location = new Vector3(-711.6313f, -918.0067f, 19.21452f);

        static Model SuspectModel = PedHash.ChiGoon02GMM;
        static Vector3 SuspectPosition = new Vector3(-709.2447f, -910.5724f, 19.2156f);
        static float SuspectHeading = 223.6788f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-706.0587f, -913.4681f, 19.21559f);
        static float ShopKeeperHeading = 97.58775f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
