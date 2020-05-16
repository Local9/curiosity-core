using System;
using System.IO;
using System.Threading.Tasks;

namespace Atlas.Bash.Modules
{
    public class ResourceConfig : IModule
    {
        public Task<int> Call(string[] args)
        {
            RecursiveSearch(new DirectoryInfo(Environment.CurrentDirectory), file => Console.WriteLine($"'{file.Replace($"{Environment.CurrentDirectory}\\", "").Replace("\\", "/")}',"));

            return Task.FromResult(0);
        }

        private void RecursiveSearch(DirectoryInfo directroy, Action<string> action)
        {
            foreach (var file in directroy.GetFiles())
            {
                action(file.FullName);
            }

            foreach (var directory in directroy.GetDirectories())
            {
                RecursiveSearch(directory, action);
            }
        }
    }
}