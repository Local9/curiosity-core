using Atlas.Bash.Extensions;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Atlas.Bash.Modules
{
    public class Install : IModule
    {
        public static readonly string NecessitiesUrl = "https://github.com/FrostyLucas/Atlas/archive/master.zip";

        public async Task<int> Call(string[] args)
        {
            var name = ConsoleExtensions.Prompt("Which name do you want your server to use?",
                new DirectoryInfo("./").Name);
            var port = ConsoleExtensions.Prompt("Which port do you want to use?", 30120);
            var tags = ConsoleExtensions.Prompt("Which tags do you want to use?", "default, roleplay");
            var listed = ConsoleExtensions.Prompt("Do you want your server to be listed in the server browser?", "y/n");
            var scripthook =
                ConsoleExtensions.Prompt("Do you want to allow players to use scripthook based mods like Lambda?",
                    "y/n");
            var maximumPlayers = ConsoleExtensions.Prompt("Which is the maximum player count allowed?", 32);
            var licensekey = ConsoleExtensions.Prompt("Which licensekey do you want to use?", "none");
            var rconPassword = ConsoleExtensions.Prompt("Which RCON password do you want to use?", "none");

            Console.WriteLine("");
            Console.WriteLine("(( Installing FiveM ))");
            Console.WriteLine("");

            await InstallFiveM("fivem");

            Console.WriteLine("");
            Console.WriteLine("(( Installing configurations and other necessities )) ");
            Console.WriteLine("");

            await InstallConfig(name, IntegerExtensions.Parse(port, 30120), tags, listed == "y" || listed == "yes",
                scripthook == "y" || scripthook == "yes",
                IntegerExtensions.Parse(maximumPlayers, 32), licensekey, rconPassword);

            Console.WriteLine("");
            Console.WriteLine("(( Installation has been completed, use `atlas start` to start the server ))");

            return 0;
        }

        public async Task InstallConfig(string name, int port, string tags, bool listed, bool scripthook,
            int maximumPlayers,
            string licensekey,
            string rconPassword)
        {
            var directory = Environment.CurrentDirectory;

            using (var client = new WebClient())
            {
                Console.WriteLine($" Downloading necessities from {NecessitiesUrl}");

                var downloaded = await client.DownloadDataTaskAsync(NecessitiesUrl);

                Console.WriteLine(" Download complete, installing the files.");

                using (var stream = new MemoryStream(downloaded))
                using (var zip = ZipFile.Read(stream))
                {
                    zip.ExtractAll(directory, ExtractExistingFileAction.OverwriteSilently);
                }

                Console.WriteLine(" Extracting downloaded files...");

                var master = new DirectoryInfo(Path.Combine(directory, "Atlas-master"));

                CopyDirectory(master, new DirectoryInfo(directory));

                master.Delete(true);

                var config = new FileInfo(Path.Combine(directory, "Atlas.cfg"));

                Console.WriteLine(" Filtering `Atlas.cfg` with settings...");

                File.WriteAllText(config.FullName, File.ReadAllText(config.FullName)
                    .Replace("${PORT}", port.ToString())
                    .Replace("${SCRIPTHOOK}", (scripthook ? 1 : 0).ToString())
                    .Replace("${TAGS}", $"\"{tags}\"")
                    .Replace("${SERVER_NAME}", $"\"{name}\"")
                    .Replace("${MAXIMUM_PLAYERS}", maximumPlayers.ToString())
                    .Replace("${RCON}",
                        string.IsNullOrEmpty(rconPassword) || rconPassword == "none"
                            ? "#rcon_password \"\""
                            : $"rcon_passwpord \"{rconPassword}\"")
                    .Replace("${SV_MASTER}", listed ? "#sv_master1 \"\"" : "sv_master1 \"\"")
                    .Replace("${LICENSE_KEY}", licensekey)
                );

                Console.WriteLine(" Installed successfully!");
            }
        }

        public async Task InstallFiveM(string path)
        {
            Console.WriteLine(" Searching for latest FiveM windows version...");

            using (var client = new WebClient())
            {
                var result = await client.DownloadStringTaskAsync(
                    "https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/");
                var regex = new Regex("href=\"(\\d{3})-([^\"]*)/\"", RegexOptions.IgnoreCase);
                var versions = new List<Tuple<string, string>>();

                for (var match = regex.Match(result); match.Success; match = match.NextMatch())
                {
                    versions.Add(new Tuple<string, string>(match.Groups[1].Value, match.Groups[2].Value));
                }

                var latest = versions.Max();

                Console.WriteLine($" Found latest build: {latest.Item1}, downloading!");

                var downloaded = await client.DownloadDataTaskAsync(
                    $"https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/{latest.Item1}-{latest.Item2}/server.zip");

                Console.WriteLine(" Download complete, installing the files.");
                Directory.CreateDirectory(path);

                using (var stream = new MemoryStream(downloaded))
                using (var zip = ZipFile.Read(stream))
                {
                    zip.ExtractAll(path, ExtractExistingFileAction.OverwriteSilently);
                }

                Console.WriteLine(" Installed successfully!");
            }
        }

        public void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var file in source.GetFiles())
            {
                var sub = new FileInfo(Path.Combine(target.FullName, file.Name));

                if (sub.Exists) sub.Delete();

                file.CopyTo(sub.FullName, true);
            }

            foreach (var directory in source.GetDirectories())
            {
                var sub = new DirectoryInfo(Path.Combine(target.FullName, directory.Name));

                if (sub.Exists) sub.Delete(true);

                CopyDirectory(directory, target.CreateSubdirectory(directory.Name));
            }
        }
    }
}