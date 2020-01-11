using System.Threading.Tasks;

namespace Atlas.Bash.Modules
{
    public interface IModule
    {
        Task<int> Call(string[] args);
    }
}