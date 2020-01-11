using System;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobBusinessProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; }
        public string Seed { get; set; }
        public Business Business { get; set; }

        public override async void Begin(Job job)
        {
            await Session.Loading();
            
            Seed = $"business::{job.Attachment.ToString().ToLower()}";
            Business = await EventSystem.GetModule().Request<Business>("business:fetch", Seed) ?? new Business
            {
                Seed = Seed,
                Balance = 0,
                Registered = DateTime.Now.Ticks
            };

            EventSystem.GetModule().Attach("business:update", new EventCallback(metadata =>
            {
                var business = metadata.Find<Business>(0);

                Business.Balance = business.Balance;

                return null;
            }));
        }

        public void Commit()
        {
            EventSystem.GetModule().Send("business:update", Business);
        }
    }
}