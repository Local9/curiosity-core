using Curiosity.Systems.Client.Managers;

namespace Curiosity.Systems.Client.Interface
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public bool IsMenuOpen { get; set; } = false;

        public override void Begin()
        {

        }
    }
}
