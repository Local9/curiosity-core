namespace Atlas.Roleplay.Client.Environment.Jobs
{
    public abstract class JobProfile
    {
        public AtlasPlugin Atlas => AtlasPlugin.Instance;
        public Job Job { get; set; }
        public abstract JobProfile[] Dependencies { get; set; }
        public abstract void Begin(Job job);
    }
}