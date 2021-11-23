using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.Managers
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("police:report:notify", new EventCallback(metadata =>
            {
                string notification = metadata.Find<string>(0);
                int vehNetId = metadata.Find<int>(1);

                if (Instance.ExportRegistry["curiosity-client"] is not null)
                {
                    Instance.ExportRegistry["curiosity-client"].Notification(4, notification, "bottom-right", "snackbar", 10000, true, false);
                }

                int entityHandle = NetworkGetEntityFromNetworkId(vehNetId);
                if (DoesEntityExist(entityHandle))
                {
                    Vehicle veh = new Vehicle(entityHandle);
                    Blip b = veh.AttachedBlip;
                    b.Color = BlipColor.Red;
                }

                return null;
            }));
        }
    }
}
