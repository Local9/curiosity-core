
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class PedNormal : NormalPed
    {
        private readonly Ped _ped;
        private float _visionDistance = 50f;

        private bool IsHatingTarget = false;

        public PedNormal(int handle, float visionDistance = 50f, float experience = 10f) : base(handle, visionDistance, experience)
        {
            this._ped = this;
            this._visionDistance = visionDistance;
            CanBeArrested = true; // randomise/control this
        }

        public PedNormal(int handle, bool canArrest = false, float visionDistance = 50f, float experience = 10f) : base(handle, canArrest, visionDistance, experience)
        {
            this._ped = this;
            this._visionDistance = visionDistance;
            CanBeArrested = true; // randomise/control this
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
