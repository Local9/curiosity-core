using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Rural.Shops
{
    class TataviamMountains
    {
        static string Name = "24/7 on Tataviam Mountains";
        static Vector3 Location = new Vector3(2561.057f, 385.2407f, 108.6211f);

        static Model SuspectModel = PedHash.Farmer01AMM;
        static Vector3 SuspectPosition = new Vector3(2553.855f, 384.7222f, 108.6229f);
        static float SuspectHeading = 210.8072f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(2556.384f, 380.7838f, 108.6229f);
        static float ShopKeeperHeading = 357.0758f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
