namespace Curiosity.Global.Shared.net.Enums
{
    public enum MissionType
    {
        STORE,
        STOLEN_VEHICLE
    }

    public enum MissionDifficulty
    {
        Unknown = 0,
        Easy = 1,
        Medium,
        Hard
    }

    public enum ObjectiveType
    {
        Unknown = 0,
        EnterVehicle,
        Checkpoint,
        CollectPickup,
        DistanceSection,
        LeaveArea,
        ClearArea,
        HealPed,
        PutoutFire
    }
}
