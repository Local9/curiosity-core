using CitizenFX.Core;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client
{
    public class Session
    {
        public static bool HasSpawned { get; set; }
        public static async Task Loading()
        {
            while (!HasSpawned)
            {
                await BaseScript.Delay(100);
            }
        }
    }
}