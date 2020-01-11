using System;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Library.LawEnforcement
{
    public class JailCase
    {
        public string Seed { get; set; }
        public long IssuedAt { get; set; }
        public long Time { get; set; }
        public Crime Crime { get; set; }
        public bool HasEscaped { get; set; }
        public JailSecurity JailSecurity { get; set; }
        public bool IsActive => IssuedAt + Time > Date.Timestamp;

        public JailCase()
        {
            Seed = Library.Seed.Generate();
        }

        public void Commit(AtlasCharacter character)
        {
            character.Metadata.JailCases.RemoveAll(self => self.Seed == Seed);
            character.Metadata.JailCases.Add(this);
        }

        public string GetDate()
        {
            return GetDate(Date.Timestamp);
        }

        public string GetDate(long date)
        {
            var timespan = new TimeSpan(Date.TimestampToTicks((IssuedAt + Time - date) * 86400));
            var result =
                $"{GetDateComponent(timespan.Days, "dagar,")}{GetDateComponent(timespan.Hours, "timmar,")}{GetDateComponent(timespan.Minutes, "minuter,")}{GetDateComponent(timespan.Seconds, "sekunder,")}"
                    .TrimEnd(' ');

            return result.EndsWith(",") ? result.Substring(0, result.Length - 1) : result;
        }


        private string GetDateComponent(int amount, string component)
        {
            return amount > 0 ? $"{amount} {component} " : " ";
        }

        public string GetSecurity()
        {
            switch (JailSecurity)
            {
                case JailSecurity.Maxiumum:
                    return "Högsta";
                case JailSecurity.Middle:
                    return "Medel";
                case JailSecurity.Minimum:
                    return "Minsta";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string[] GetSecurityLevels()
        {
            return new[]
            {
                "Högsta",
                //"Medel",
                "Minsta"
            };
        }
    }
}