using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Server.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("vehicle:log:player", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalVehicle = netId;
                Logger.Debug($"vehicle:log:player -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            //EventSystem.GetModule().Attach("vehicle:spawn", new EventCallback(metadata =>
            //{
            //    if (arguments.Count <= 0) return;
            //    var model = API.GetHashKey(arguments.ElementAt(0));
            //    float x = arguments.ElementAt(1).ToFloat();
            //    float y = arguments.ElementAt(2).ToFloat();
            //    float z = arguments.ElementAt(3).ToFloat();

            //    Vector3 pos = new Vector3(x, y, z);
            //    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
            //    return null
            //}));
        }
    }
}
