using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Rural.Shops
{
    class BanhamCanyonTwentyFour
    {
        static string Name = "24/7 on Banham Canyon";
        static Vector3 Location = new Vector3(-3036.43f, 589.7015f, 7.811507f);

        static Model SuspectModel = PedHash.Surfer01AMY;
        static Vector3 SuspectPosition = new Vector3(-3043.355f, 586.9014f, 7.908933f);
        static float SuspectHeading = 239.2633f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(-3038.999f, 584.452f, 7.908933f);
        static float ShopKeeperHeading = 1.585089f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
