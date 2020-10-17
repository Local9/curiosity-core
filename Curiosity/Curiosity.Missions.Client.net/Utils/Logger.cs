using System.Diagnostics;
using System.Reflection;

namespace Curiosity.Missions.Client.net.Utils
{
    class Logger
    {
        public static void Log(string message)
        {
#if DEBUG
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            string caller = $"{method.ReflectedType?.FullName}.{method.Name}";
            Debug.WriteLine($"{caller}: {message}");
#endif
        }
    }
}
