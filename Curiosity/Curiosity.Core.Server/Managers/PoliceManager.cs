using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Managers
{
    public class PoliceManager : Manager<PoliceManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("cs:police:ticket", new AsyncEventCallback(async metadata => {
                ExportMessage em = new ExportMessage();
                
                try
                {
                    float speed = metadata.Find<float>(0);
                    float speedLimit = metadata.Find<float>(1);
                    bool informPolice = metadata.Find<bool>(2);
                    int vehicleNetId = metadata.Find<int>(3);
                    // add ticket to the database
                    // get Vehicle
                    int vehicleHandle = NetworkGetEntityFromNetworkId(vehicleNetId);
                    Vehicle vehicle = new Vehicle(vehicleHandle);

                    if (vehicle is null)
                    {
                        em.error = "Vehicle not found.";
                        goto RETURN_MESSAGE;
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"cs:police:ticket");
                    em.error = $"Error when invoicing speeding ticket.";
                }

            RETURN_MESSAGE:
                return em;
            }));
        }
    }
}
