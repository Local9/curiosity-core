using System.Collections.Generic;
using System.Linq;
using Atlas.Roleplay.Client.Environment.Jobs.Ambulance;
using Atlas.Roleplay.Client.Environment.Jobs.Bennys;
using Atlas.Roleplay.Client.Environment.Jobs.Exceptions;
using Atlas.Roleplay.Client.Environment.Jobs.Police;
using Atlas.Roleplay.Client.Environment.Jobs.Taxi;
using Atlas.Roleplay.Client.Managers;

namespace Atlas.Roleplay.Client.Environment.Jobs
{
    public class JobManager : Manager<JobManager>
    {
        public List<Job> Registered { get; set; } = new List<Job>();

        public override void Begin()
        {
            RegisterJob(new PoliceJob());
            RegisterJob(new AmbulanceJob());
            RegisterJob(new BennysJob());
            RegisterJob(new TaxiJob());
        }

        public async void RegisterJob(Job job)
        {
            if (Registered.Contains(job)) return;

            Registered.Add(job);

            foreach (var profile in job.Profiles)
            {
                profile.Job = job;
                profile.Begin(job);

                if (profile.Dependencies == null) continue;

                foreach (var dependency in profile.Dependencies)
                {
                    if (job.Profiles.FirstOrDefault(self => self.GetType() == dependency.GetType()) == null)
                    {
                        throw new JobException(job,
                            $"Profile `{profile.GetType().Name}` must have the dependency `{dependency.GetType().Name}` in order to function correctly.");
                    }
                }
            }

            foreach (var blip in job.Blips)
            {
                blip.Commit();
            }

            await Session.Loading();

            job.Begin();
            
            AtlasPlugin.Instance.AttachTickHandlers(job);
        }

        public T GetJob<T>() where T : Job
        {
            return (T) Registered.FirstOrDefault(self => self.GetType() == typeof(T));
        }
    }
}