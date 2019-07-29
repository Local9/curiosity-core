using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Country.Shops
{
    class GrandSenoraDesertTwentyFour
    {
        static string Name = "24/7, Grand Senora Desert";
        static Vector3 Location = new Vector3(2683.266f, 3281.896f, 55.24052f);

        static Model SuspectModel = PedHash.Hillbilly02AMM;
        static Vector3 SuspectPosition = new Vector3(2676.935f, 3284.402f, 55.24114f);
        static float SuspectHeading = 185.1518f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(2676.009f, 3280.396f, 55.24113f);
        static float ShopKeeperHeading = 335.5503f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
