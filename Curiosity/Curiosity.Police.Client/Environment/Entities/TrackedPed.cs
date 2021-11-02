using CitizenFX.Core;

namespace Curiosity.Police.Client.Environment.Entities
{
    internal class TrackedPed
    {
        public Ped ped;
        public bool near = false;

        public TrackedPed(Ped ped) => this.ped = ped;
    }
}
