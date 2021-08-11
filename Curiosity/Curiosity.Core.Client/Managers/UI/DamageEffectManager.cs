using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.UI
{
    public class DamageEffectManager : Manager<DamageEffectManager>
    {
        int _updateTick = 0;
        public int Ticks = 60;
        
        public float EffectThreshold = .5f;
        public float SpeedIn = .8f;
        public float SpeedOut = .2f;
        
        string _effect = "rply_saturation_neg";
        string _currentEffect = "rply_saturation_neg";
        public List<dynamic> Effects = new List<dynamic>()
        {
            new { Label = "Black and White", Effect = "rply_saturation_neg" },
            new { Label = "High Contrast B/W", Effect = "hud_def_desatcrunch" },
            new { Label = "High Contrast B/W Blur", Effect = "mp_death_grade_blend01" },
            new { Label = "Desaturation with Bloom", Effect = "dying" },
            new { Label = "Red Boarders", Effect = "damage" },
            new { Label = "Red", Effect = "glasses_red" },
        };

        int _healthDead = 2;

        float _beforeFraction = 0f;
        float _fraction = 0f;
        float _targetFraction = 0f;


        public override void Begin()
        {
            string savedEffect = GetResourceKvpString("cur:damage:effect");
            foreach(dynamic i in Effects)
            {
                if (i.Label == savedEffect)
                    _effect = i.Effect;
            }
        }

        public void SetEffect(string saveLabel, string effect)
        {
            _effect = effect;
            SetResourceKvp("cur:damage:effect", saveLabel);
        }

        public string Effect => _effect;

        [TickHandler(SessionWait = true)]
        private async Task OnDamageEffectTick()
        {
            int gameTimer = GetGameTimer();

            bool shouldRun = Ticks <= 0 ? false : _updateTick < gameTimer;

            if (shouldRun)
            {
                _updateTick = gameTimer + 1000 / Ticks;
                float playerHealth = Game.PlayerPed.Health;
                float playerMaxHealth = Game.PlayerPed.MaxHealth;
                _targetFraction = Math.Min(Math.Max(1f - (playerHealth - _healthDead) / (playerMaxHealth - _healthDead) / EffectThreshold, 0f), 1f);

                if (playerHealth <= _healthDead && !Game.PlayerPed.IsDead)
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

                if (_beforeFraction != _fraction || _currentEffect != _effect)
                {
                    SetTimecycleModifier(_effect);

                    if (_fraction < 0.05)
                    {
                        SetTimecycleModifierStrength(0f);
                    }
                    else
                    {
                        SetTimecycleModifierStrength(_fraction);
                    }
                    
                    _beforeFraction = _fraction;
                    _currentEffect = _effect;
                }
            }
        }
    }
}
