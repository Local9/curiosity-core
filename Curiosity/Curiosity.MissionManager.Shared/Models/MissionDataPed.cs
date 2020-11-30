namespace Curiosity.Systems.Shared.Entity
{
    public class MissionDataPed : MissionDataEntity
    {
        public bool IsSuspect { get; internal set; }
        public bool IsHandcuffed { get; internal set; }
        public override string ToString()
        {
            return $"Ped: Suspect: {IsSuspect}, IsHandcuffed: {IsHandcuffed}, Blip: {AttachBlip}";
        }
    }
}