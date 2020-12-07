using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Interface;
using System.Collections.Generic;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Managers
{
    public class TrafficStopManager : Manager<TrafficStopManager>
    {
        public static TrafficStopManager Manager;

        public Vehicle tsVehicle { get; private set; }
        public Ped tsDriver { get; private set; }

        public List<Ped> tsPassengers { get; private set; } = new List<Ped>();

        public override void Begin()
        {
            Manager = this;
        }

        public async void SetVehicle(Vehicle vehicle)
        {
            tsVehicle = vehicle;
            tsVehicle.IsImportant = true;
            tsVehicle.IsMission = true;

            tsVehicle.AttachSuspectBlip();

            await BaseScript.Delay(100);

            tsDriver = new Ped(vehicle.Fx.Driver);

            Logger.Debug($"Traffic Stop -> Ped {tsDriver.Handle}");

            tsDriver.IsMission = true;
            tsDriver.IsSuspect = true;
            tsDriver.IsImportant = true;

            foreach(CitizenFX.Core.Ped ped in tsVehicle.Fx.Passengers)
            {
                if (ped != tsDriver)
                    tsPassengers.Add(new Ped(ped));

                await BaseScript.Delay(100);
            }

            await BaseScript.Delay(100);

            StartMission();
        }

        void StartMission()
        {
            string missionId = $"misTrafficStop";

            Mission.missions.ForEach(mission =>
            {
                MissionInfo missionInfo = Functions.GetMissionInfo(mission);

                if (missionInfo == null)
                {
                    Notify.Error("Mission Info Attribute not found.");
                    return;
                }

                if (missionInfo.id == missionId)
                {
                    EventSystem.Request<bool>("mission:activate", missionInfo.id, missionInfo.unique, missionInfo.displayName);

                    Functions.StartMission(mission, "Performing a traffic stop");
                }
            });
        }

        public void ClearVehicle()
        {
            tsVehicle = null;
            tsDriver = null;
        }
    }
}
