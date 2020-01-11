using System;
using System.IO;

namespace Atlas.Bash.Utilities
{
    public class PathResolver
    {
        public static readonly string ServerFile = "FXServer.exe";
        public static readonly string ConfigFile = "atlas.cfg";

        public static string FindServer()
        {
            if (new FileInfo(Path.Combine(Environment.CurrentDirectory, "fivem", ServerFile)).Exists)
                return Path.Combine(Environment.CurrentDirectory);

            throw new FileNotFoundException("Unable to locate server in the directory tree", ServerFile);
        }
    }
}