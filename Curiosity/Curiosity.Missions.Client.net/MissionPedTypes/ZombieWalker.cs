
using CitizenFX.Core;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Utils;

namespace Curiosity.Missions.Client.MissionPedTypes
{
    internal class ZombieWalker : ZombiePed
    {
        private readonly Ped _ped;

        public override string MovementStyle { get; set; } = "move_m@drunk@verydrunk";

        public ZombieWalker(int handle) : base(handle)
        {
            // CitizenFX.Core.UI.Screen.ShowNotification($"Ped Walker: {handle}");
            this._ped = this;
        }

        public override void OnAttackTarget(Ped target)
        {
            if (target.IsDead)
            {
                if (!this._ped.IsPlayingAnim("amb@world_human_bum_wash@male@high@idle_a", "idle_b"))
                {
                    this._ped.Task.PlayAnimation("amb@world_human_bum_wash@male@high@idle_a", "idle_b", 8f, -1, 1);
                }
            }
            else if (!this._ped.IsPlayingAnim("rcmbarry", "bar_1_teleport_aln"))
            {
                this._ped.Task.PlayAnimation("rcmbarry", "bar_1_teleport_aln", 8f, 1000, 16);
                if (!target.IsInvincible)
                {
                    target.ApplyDamage(ZombiePed.ZombieDamage);
                }
                base.InfectTarget(target);
            }
        }

        public override void OnGoToTarget(Ped target)
        {
            this._ped.Task.GoTo(target);
        }
    }
}
