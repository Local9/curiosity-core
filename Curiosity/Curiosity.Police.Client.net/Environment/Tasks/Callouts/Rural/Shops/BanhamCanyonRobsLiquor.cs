using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Rural.Shops
{
    class BanhamCanyonRobsLiquor
    {
        static string Name = "Rob's Liquor, Banham Canyon";
        static Vector3 Location = new Vector3(-2974.973f, 390.8409f, 15.03081f);

        static Model SuspectModel = PedHash.StrPunk01GMY;
        static Vector3 SuspectPosition = new Vector3(-2968.926f, 392.5486f, 15.04331f);
        static float SuspectHeading = 161.5946f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-2966.403f, 390.5244f, 15.04331f);
        static float ShopKeeperHeading = 80.79575f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
