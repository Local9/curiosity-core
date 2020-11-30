namespace Curiosity.Systems.Shared.Entity
{
    public class MissionDataVehicle : MissionDataEntity
    {
        public bool IsTowable { get; internal set; }

        public override string ToString()
        {
            return $"Vehicle: Towable: {IsTowable}, Blip: {AttachBlip}";
        }
    }
}