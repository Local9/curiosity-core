using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.City.WestMirrorDrive
{
    class MirrorParkShop
    {
        static string Name = "LTD on West Mirror Drive";
        static Vector3 Location = new Vector3(1159.815f, -327.377f, 69.21338f);

        static Model SuspectModel = PedHash.Dockwork01SMY;
        static Vector3 SuspectPosition = new Vector3(1160.791f, -320.9355f, 69.20513f);
        static float SuspectHeading = 224.5752f;

        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(1164.704f, -322.4249f, 69.20515f);
        static float ShopKeeperHeading = 109.8184f;

        public static bool Init()
        {
            Classes.CreateShopCallout.StartCallout(Name, Location, SuspectModel, SuspectPosition, SuspectHeading, ShopKeeperModel, ShopKeeperPosition, ShopKeeperHeading);
            return true;
        }
    }
}
