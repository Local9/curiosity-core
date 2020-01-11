namespace Atlas.Roleplay.Library.LawEnforcement
{
    public class Charge
    {
        public string Seed { get; set; }
        public string Label { get; set; }
        public int Time { get; set; }

        public override string ToString()
        {
            return Label;
        }
    }
}