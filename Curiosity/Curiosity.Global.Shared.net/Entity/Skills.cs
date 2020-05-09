namespace Curiosity.Global.Shared.net.Entity
{
    public class Skills
    {
        public int Id;
        public Enums.SkillType TypeId;
        public string Description;
        public string Label;
        public string LabelDescription;
        public int Value;
    }

    public class MissionCompleted
    {
        public bool Passed = false;
    }

    public class SkillMessage
    {
        public string PlayerHandle;
        public string Skill;
        public bool MissionPed;
        public bool Increase;
        public bool IsHeadshot;
    }
}
