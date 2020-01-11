using System;

namespace Atlas.Roleplay.Client.Environment.Jobs.Exceptions
{
    public class JobException : Exception
    {
        public JobException(Job job, string message) : base($"[Job] [{job.Attachment.ToString()}] {message}")
        {
        }
    }
}