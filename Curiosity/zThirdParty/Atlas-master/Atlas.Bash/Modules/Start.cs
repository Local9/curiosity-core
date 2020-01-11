using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Atlas.Bash.Utilities;

namespace Atlas.Bash.Modules
{
    public class Start : IModule
    {
        public Task<int> Call(string[] args)
        {
            try
            {
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo(
                        Path.Combine(PathResolver.FindServer(), "fivem", PathResolver.ServerFile),
                        $"+set citizen_dir {Path.GetFullPath("fivem/citizen")} +exec {PathResolver.ConfigFile}")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        ErrorDialog = false,
                        WorkingDirectory = PathResolver.FindServer()
                    }
                })
                {
                    process.ErrorDataReceived += (sender, @event) => Console.WriteLine(@event.Data);
                    process.Start();
                    process.BeginErrorReadLine();

                    new Thread(() =>
                    {
                        char character;

                        while (process != null && !process.HasExited &&
                               (character = (char) process.StandardOutput.Read()) >= 0)
                        {
                            Console.Write(character);
                        }
                    }).Start();

                    process.WaitForExit();

                    return Task.FromResult(0);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("An unhandled application error has occured:");
                Console.WriteLine(exception.Message);

                if (exception.InnerException != null) Console.WriteLine(exception.InnerException.Message);

                return Task.FromResult(0);
            }
        }
    }
}