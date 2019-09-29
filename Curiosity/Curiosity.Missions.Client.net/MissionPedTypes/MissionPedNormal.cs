﻿
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class MissionPedNormal : MissionPed
    {
        private readonly Ped _ped;

        public MissionPedNormal(int handle) : base(handle)
        {
            this._ped = this;
        }

        public override void OnAttackTarget(Ped target)
        {
            
        }

        public override void OnGoToTarget(Ped target)
        {
            this._ped.Task.GoTo(target);
        }
    }
}
