using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.VespucciCanals
{
    class SanAndreasAveShop
    {
        static string Name = "24/7 on San Andreas Ave";
        static Vector3 Location = new Vector3(-1226.846f, -901.4744f, 12.28888f);

        static Model SuspectModel = PedHash.ChiCold01GMM;
        static Vector3 SuspectPosition = new Vector3(-1224.359f, -905.7396f, 12.32636f);
        static float SuspectHeading = 233.6525f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-1226.846f, -901.4744f, 12.28888f);
        static float ShopKeeperHeading = 233.4752f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
