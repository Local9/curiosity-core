using Atlas.Bash.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Bash
{
    public class Atlas
    {
        public static readonly string CommandUsage = "Usage: atlas <install, start, files>";
        public static int ExitCode = -1;

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(CommandUsage);

                return;
            }

            var command = args[0];
            var sliced = args.Skip(1).Take(args.Length - 1).ToArray();

            switch (command.ToUpper())
            {
                case "INSTALL":
                    Task.Run(async () => { ExitCode = await new Install().Call(sliced); });

                    break;
                case "START":
                    Task.Run(async () => { ExitCode = await new Start().Call(sliced); });

                    break;
                case "FILES":
                    Task.Run(async () => { ExitCode = await new ResourceConfig().Call(sliced); });

                    break;
                default:
                    Console.WriteLine(CommandUsage);

                    break;
            }

            while (ExitCode == -1)
            {
                // Stop automatic exit
            }
        }
    }
}