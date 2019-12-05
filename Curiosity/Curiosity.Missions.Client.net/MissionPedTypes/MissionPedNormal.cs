using CitizenFX.Core;
using Curiosity.Missions.Client.net.MissionPeds;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class MissionPedNormal : MissionPed
    {
        private readonly Ped _ped;
        private float _visionDistance = 50f;

        private bool IsHatingTarget = false;

        public MissionPedNormal(int handle, float visionDistance = 50f, float experience = 10f) : base(handle, visionDistance, experience)
        {
            this._ped = this;
            this._visionDistance = visionDistance;
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
