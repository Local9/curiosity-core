using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Downtown
{
    class ClintonAveShop
    {
        static string Name = "24/7 on Clinton Ave";
        static Vector3 Location = new Vector3(377.0479f, 324.0891f, 103.5665f);

        static Model SuspectModel = PedHash.ChiCold01GMM;
        static Vector3 SuspectPosition = new Vector3(375.6602f, 325.6703f, 103.5664f);
        static float SuspectHeading = 255.8121f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(372.8627f, 328.1952f, 103.5664f);
        static float ShopKeeperHeading = 255.3216f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
