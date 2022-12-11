using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Managers.Events;

namespace Curiosity.Framework.Client.Managers.GamePlayer
{
    public class PlayerDeathManager : Manager<PlayerDeathManager>
    {
        List<string> _deathEffects = new()
        {
            "rply_saturation_neg",
            "hud_def_desatcrunch",
            "mp_death_grade_blend01",
            "dying",
            "damage",
            "glasses_red"
        };

        public override void Begin()
        {
            InternalGameEvents.OnPlayerKilledByPlayer += OnPlayerKilledByPlayer;
            InternalGameEvents.OnPedDied += OnPedDied;
        }

        private void OnPedDied(int ped, int attacker, uint weaponHash, bool isMeleeDamage)
        {
            if (attacker > -1) return;
            if (ped != Game.PlayerPed.Handle) return;

            ShowMpWastedMessage("You Died");
            ResurrectLocalPlayer(Game.PlayerPed.Position);
        }

        private void OnPlayerKilledByPlayer(int victimPlayer, int killerPlayer, uint weaponHash, bool isMeleeDamage)
        {
            if (killerPlayer == Game.Player.Handle) return;
            ShowMpWastedMessage("Wasted");
            ResurrectLocalPlayer(Game.PlayerPed.Position);
        }

        private void ShowMpWastedMessage(string message, string subtitle = "")
        {
            ScaleformUI.ScaleformUI.BigMessageInstance.ShowMpWastedMessage(message, subtitle);
            PlaySoundFrontend(-1, "TextHit", "WastedSounds", true);
            SetTimecycleModifier(_deathEffects[PluginManager.Random.Next(_deathEffects.Count)]);
            SetTimecycleModifierStrength(.5f);
        }

        private async void ResurrectLocalPlayer(Vector3 position)
        {
            await BaseScript.Delay(4000);

            Ped playerPed = Game.PlayerPed;
            playerPed.FadeEntity(false);
            playerPed.Weapons.Select(WeaponHash.Unarmed);

            await GameInterface.Hud.FadeOut(500);

            await BaseScript.Delay(500);

            NetworkResurrectLocalPlayer(position.X, position.Y, position.Z, 0f, true, false);
            NetworkConcealPlayer(Game.Player.Handle, true, true);

            SetTimecycleModifier("default");
            SetTimecycleModifierStrength(1f);

            await BaseScript.Delay(3000);

            playerPed.ClearBloodDamage();
            playerPed.ClearLastWeaponDamage();

            playerPed.FadeEntity(true, slow: false);

            NetworkConcealPlayer(Game.Player.Handle, false, true);

            GameInterface.Hud.FadeIn(500);
        }
    }
}
