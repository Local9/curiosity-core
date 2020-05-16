using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Roleplay.Library.LawEnforcement
{
    public class Crime
    {
        public List<Charge> Charges { get; set; } = new List<Charge>();
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public override string ToString()
        {
            var modified = new Dictionary<Charge, int>();

            Charges.ForEach(self =>
            {
                if (modified.ContainsKey(self))
                {
                    modified[self] += 1;
                }
                else
                {
                    modified.Add(self, 1);
                }
            });

            return $"{string.Join(", ", modified.Select(self => $"{self.Key} x{self.Value}"))} ({IssuedAt:MM/dd/yyyy HH:mm:ss})";
        }
    }
}