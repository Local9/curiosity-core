using CitizenFX.Core;

namespace Curiosity.Menus.Client.net.Classes.Data
{
    class CompanionData
    {
        public string Label;
        public PedHash PedHash;
        public bool IsHuman;
        public bool CanInteract;

        public CompanionData(string label, PedHash pedHash, bool isHuman = false, bool canInteract = false)
        {
            Label = label;
            PedHash = pedHash;
            IsHuman = isHuman;
            CanInteract = canInteract;
        }
    }
}
