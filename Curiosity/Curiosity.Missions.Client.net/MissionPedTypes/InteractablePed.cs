using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.MissionPeds;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class InteractablePed : InteractivePed
    {
        private readonly Ped _ped;
        private float _visionDistance = 50f;

        private bool IsHatingTarget = false;

        public InteractablePed(int handle) : base(handle)
        {
            this._ped = this;
        }

        public override void OnAttackTarget(Ped target)
        {
            if (!IsHatingTarget)
            {
                this._ped.Task.FightAgainstHatedTargets(_visionDistance);
                IsHatingTarget = true;
            }
        }

        public override void OnGoToTarget(Ped target)
        {
            this._ped.Task.GoTo(target);
            IsHatingTarget = false;
        }
    }
}
