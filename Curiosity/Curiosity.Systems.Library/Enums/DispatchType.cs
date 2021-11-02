namespace Curiosity.Systems.Library.Enums
{
    public enum DispatchType
	{
		DT_Invalid,
		DT_PoliceAutomobile,
		DT_PoliceHelicopter,
		DT_FireDepartment,
		DT_SwatAutomobile,
		DT_AmbulanceDepartment,
		DT_PoliceRiders,
		DT_PoliceVehicleRequest,
		DT_PoliceRoadBlock,
		DT_PoliceAutomobileWaitPulledOver,
		DT_PoliceAutomobileWaitCruising,
		DT_Gangs,
		DT_SwatHelicopter,
		DT_PoliceBoat,
		DT_ArmyVehicle,
		DT_BikerBackup
	};

	public static class Dispatch
    {
		public static DispatchType[] PoliceForces = new DispatchType[11] {
			DispatchType.DT_PoliceAutomobile,
			DispatchType.DT_PoliceHelicopter,
			DispatchType.DT_SwatAutomobile,
			DispatchType.DT_PoliceRiders,
			DispatchType.DT_PoliceVehicleRequest,
			DispatchType.DT_PoliceRoadBlock,
			DispatchType.DT_PoliceAutomobileWaitPulledOver,
			DispatchType.DT_PoliceAutomobileWaitCruising,
			DispatchType.DT_SwatHelicopter,
			DispatchType.DT_PoliceBoat,
			DispatchType.DT_ArmyVehicle
		};
	}
}
