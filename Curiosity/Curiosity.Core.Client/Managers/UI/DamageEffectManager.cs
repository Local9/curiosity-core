using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.UI
{
    public class DamageEffectManager : Manager<DamageEffectManager>
    {
        int _updateTick = 0;
        public int Ticks = 60;
        
        public float EffectThreshold = .5f;
        public float SpeedIn = .8f;
        public float SpeedOut = .2f;
        
        public string Effect = "rply_saturation_neg";
        public List<dynamic> Effects = new List<dynamic>()
        {
            new { Label = "Red Boarders", Effect = "damage" },
            new { Label = "Red", Effect = "glasses_red" },
            new { Label = "Desaturation with Bloom", Effect = "dying" },
            new { Label = "Black and White", Effect = "rply_saturation_neg" },
            new { Label = "High Contrast B/W", Effect = "hud_def_desatcrunch" },
            new { Label = "High Contrast B/W Blur", Effect = "mp_death_grade_blend01" },
        };

        int _healthDead = 100;

        Ped PlayerPed = Game.PlayerPed;

        float _beforeFraction = -1f;
        float _fraction = 0f;
        float _targetFraction = 0f;


        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async void OnDamageEffectTick()
        {
            int gameTimer = GetGameTimer();

            bool shouldRun = Ticks <= 0 ? false : _updateTick < gameTimer;

            if (shouldRun)
            {
                _updateTick = gameTimer + 1000 / Ticks;
                float playerHealth = PlayerPed.HealthFloat;
                float playerMaxHealth = PlayerPed.MaxHealthFloat;
                _targetFraction = Math.Min(Math.Max(1f - (playerHealth - _healthDead) / (playerMaxHealth - _healthDead) / EffectThreshold, 0f), 1f);

                if (playerHealth <= _healthDead)
                {
                    _fraction = _targetFraction;
                }
                                
                if (_targetFraction > _fraction)
                {
                    _fraction = Math.Min(_fraction + SpeedIn / Ticks, _targetFraction);
                }

                if (_targetFraction < _fraction)
                {
                    _fraction = Math.Max(_fraction - SpeedOut / Ticks, _targetFraction);
                }

                if (_beforeFraction != _fraction)
                {
                    SetTimecycleModifier(Effect);
                    SetTimecycleModifierStrength(_fraction);
                    _beforeFraction = _fraction;
                }
            }

            await BaseScript.Delay(0);
        }
    }
}
