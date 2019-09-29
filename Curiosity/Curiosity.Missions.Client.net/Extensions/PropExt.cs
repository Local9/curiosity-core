
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Extensions
{
    static class PropExt
    {
        public static bool GetDoorLockState(this Prop prop)
        {
            unsafe
            {
                bool flag = false;
                int num = 0;
                Function.Call(unchecked((Hash)(-1314587405265718273L)), new InputArgument[] { prop.Model.Hash, prop.Position.X, prop.Position.Y, prop.Position.Z, &flag, &num });
                return flag;
            }
        }

        public static void SetStateOfDoor(this Prop prop, bool locked, DoorState heading)
        {
            Function.Call(unchecked((Hash)(-563637040166458307L)), new InputArgument[] { prop.Model.Hash, prop.Position.X, prop.Position.Y, prop.Position.Z, locked, heading, 1 });
        }
    }
}
