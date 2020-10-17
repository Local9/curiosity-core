using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Utils;

namespace Curiosity.Missions.Client.MissionPedTypes
{
    class ZombieRunner : ZombiePed
    {
        private readonly Ped _ped;

        private bool _jumpAttack;

        public override string MovementStyle { get; set; } = "move_m@injured";

        public override bool PlayAudio { get; set; } = true;

        public ZombieRunner(int handle) : base(handle)
        {
            // CitizenFX.Core.UI.Screen.ShowNotification($"Ped Runner: {handle}");
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
            else if (((PluginManager.Random.NextDouble() >= 0.300000011920929 ? true : this._jumpAttack) || target.IsPerformingStealthKill || target.IsGettingUp ? false : !target.IsRagdoll))
            {
                this._ped.Task.Jump();
                Ped ped = this._ped;
                Vector3 position = target.Position - this.Position;
                this._jumpAttack = true;
                target.SetToRagdoll(2000);
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
            Function.Call((Hash)7640095384362883202L, new InputArgument[] { this._ped.Handle, target.Handle, -1, 0f, 5f, 1073741824, 0 });
        }
    }
}
