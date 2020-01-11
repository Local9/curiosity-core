using System.Collections.Generic;
using System.Linq;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs
{
    public abstract class Job
    {
        public abstract Employment Attachment { get; set; }
        public abstract string Label { get; set; }
        public abstract BlipInfo[] Blips { get; set; }
        public abstract Dictionary<int, string> Roles { get; set; }
        public abstract JobProfile[] Profiles { get; set; }

        public virtual void Begin()
        {
            // Ignored
        }

        public T GetProfile<T>() where T : JobProfile
        {
            return (T) Profiles.FirstOrDefault(self => self.GetType() == typeof(T));
        }
    }
}