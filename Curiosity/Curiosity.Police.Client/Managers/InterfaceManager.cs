using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.Managers
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        const string REPLACE_MAKE_NAME = "MAKE_NAME";

        public override void Begin()
        {
            EventSystem.Attach("police:notify", new EventCallback(metadata =>
            {
                int notificationType = metadata.Find<int>(0);
                string notificationMessage = metadata.Find<string>(1);
                int notificationDuration = metadata.Find<int>(2);
                int notificationVehicle = metadata.Find<int>(3);

                string vehicleName = "Unknown";

                if (notificationVehicle > 0)
                {
                    int entityHandle = NetworkGetEntityFromNetworkId(notificationVehicle);
                    if (DoesEntityExist(entityHandle))
                    {
                        Vehicle vehicle = new Vehicle(entityHandle);
                        string name = vehicle.DisplayName;
                        vehicleName = Game.GetGXTEntry(name);
                        notificationMessage.Replace(REPLACE_MAKE_NAME, vehicleName);
                    }
                }

                if (Instance.ExportRegistry["curiosity-client"] is not null)
                {
                    Instance.ExportRegistry["curiosity-client"].Notification(notificationType, notificationMessage, "bottom-right", "snackbar", notificationDuration, true, false);
                }

                return null;
            }));
        }
    }
}
