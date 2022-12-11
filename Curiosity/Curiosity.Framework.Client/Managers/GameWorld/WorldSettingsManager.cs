using CitizenFX.Core.UI;

namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldSettingsManager : Manager<WorldSettingsManager>
    {
        private int MP_STUN_GUN;
        private int PlayerHandle;
        
        public override void Begin()
        {
            PlayerHandle = Game.Player.Handle;
            NetworkSetFriendlyFireOption(true);
            SetPlayerVehicleDefenseModifier(PlayerHandle, 1f);
            MP_STUN_GUN = GetHashKey("WEAPON_STUNGUN_MP");
        }

        [TickHandler]
        private async Task OnGeneralTick()
        {
            int vehicleWeapon = -1553120962;
            SetWeaponDamageModifierThisFrame((uint)vehicleWeapon, 0.0f);
            SetWeaponDamageModifierThisFrame((uint)WeaponHash.StunGun, 0.0f);
            SetWeaponDamageModifierThisFrame((uint)MP_STUN_GUN, 0.0f);
            
            Screen.Hud.HideComponentThisFrame(HudComponent.Cash);
            Screen.Hud.HideComponentThisFrame(HudComponent.CashChange);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpCash);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpTagCashFromBank);
            Screen.Hud.HideComponentThisFrame(HudComponent.Saving);

            SetTextChatEnabled(false);
            DisablePlayerVehicleRewards(PlayerHandle);

            Game.Player.SetRunSpeedMultThisFrame(1f); // Speed hack to death            
        }
    }
}
