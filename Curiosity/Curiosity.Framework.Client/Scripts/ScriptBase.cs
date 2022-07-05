using Curiosity.Framework.Client.Events;

namespace Curiosity.Framework.Client.Scripts
{
    internal class ScriptBase
    {
        public PluginManager Instance => PluginManager.Instance;
        public ClientGateway ClientGateway => Instance.ClientGateway;
    }
}
