namespace Curiosity.Global.Shared.net.Enums
{
    public enum MissionType
    {
        Unknown = 0,
        Police = 1,
        Medic,
        Firefighter
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
