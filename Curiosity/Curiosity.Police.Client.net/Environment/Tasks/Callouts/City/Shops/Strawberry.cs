using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.Shops
{
    class Strawberry
    {
        static string Name = "24/7, Elgin Ave";
        static Vector3 Location = new Vector3(29.32283f, -1349.734f, 29.32919f);

        static Model SuspectModel = PedHash.AlDiNapoli;
        static Vector3 SuspectPosition = new Vector3(28.28516f, -1343.619f, 29.49703f);
        static float SuspectHeading = 121.8859f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(24.48178f, -1346.606f, 29.49703f);
        static float ShopKeeperHeading = 252.9473f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
