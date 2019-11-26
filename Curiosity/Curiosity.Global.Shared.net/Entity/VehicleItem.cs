namespace Curiosity.Global.Shared.net.Entity
{
    public class VehicleItem
    {
        public string Name;
        public string VehicleHashString;
        public float SpawnPositionX;
        public float SpawnPositionY;
        public float SpawnPositionZ;
        public float SpawnHeading;
        public long UnlockRequirementValue;
        public string UnlockRequiredSkill;
        public string UnlockRequiredSkillDescription;
        public bool InstallSirens;

        public override string ToString()
        {
            return $"Name: {Name}, Hash: {VehicleHashString}, Skill: {UnlockRequiredSkill}, Req: {UnlockRequirementValue}";
        }
    }
}
