using Newtonsoft.Json;

namespace Curiosity.System.Library.Utilities
{
    public class Clipboard
    {
        public static T Process<T>(T skeleton) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(skeleton));
        }
    }
}