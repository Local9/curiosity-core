using CitizenFX.Core.UI;

namespace Curiosity.Framework.Client.Managers
{
    public class InitiationManager : Manager<InitiationManager>
    {
        private int MP_STUN_GUN;

        public override void Begin()
        {
            AddChameleonPaintLabels();

            MP_STUN_GUN = GetHashKey("WEAPON_STUNGUN_MP");

            DecorRegister("PED_OWNER", 3);
            DecorRegister("Player_Vehicle", 3); // Required to allow the Vehicles radio to work correctly.
        }

        private static void AddChameleonPaintLabels()
        {
            AddTextEntry("G9_PAINT05", "Vice City"); // 0x03235520
            AddTextEntry("G9_PAINT13", "Kamen Rider"); // 0x06019DB0
            AddTextEntry("G9_PAINT14", "Chromatic Aberration"); // 0x0FAFB10C
            AddTextEntry("G9_PAINT15", "It's Christmas!"); // 0x1C9FCAEC
            AddTextEntry("G9_PAINT08", "Maisonette 9 Throwback"); // 0x2A6D23B3
            AddTextEntry("G9_PAINT02", "Night & Day"); // 0x2E6BABA4
            AddTextEntry("G9_PAINT16", "Temperature"); // 0x2E626E71
            AddTextEntry("G9_PAINT09", "Bubblegum"); // 0x3C1EC716
            AddTextEntry("G9_PAINT01", "Monochrome"); // 0x72A83420
            AddTextEntry("G9_PAINT03", "The Verlierer"); // 0x401A4F01
            AddTextEntry("G9_PAINT11", "Sunset"); // 0x5984C4B9
            AddTextEntry("G9_PAINT12", "The Seven"); // 0x8346183B
            AddTextEntry("G9_PAINT10", "Full Rainbow"); // 0x48712292
            AddTextEntry("G9_PAINT06", "Synthwave Nights"); // 0xBE32CB2C
            AddTextEntry("G9_PAINT07", "Four Seasons"); // 0xD07D6FC1
            AddTextEntry("G9_PAINT04", "Sprunk Extreme"); // 0xF175B1C5
        }

        [TickHandler]
        private async Task OnGeneralTickAsync()
        {
            int vehicleWeapon = -1553120962; // Vehicle Collision with Peds
            API.SetWeaponDamageModifierThisFrame((uint)vehicleWeapon, 0.0f);
            API.SetWeaponDamageModifierThisFrame((uint)WeaponHash.StunGun, 0.0f);
            API.SetWeaponDamageModifierThisFrame((uint)MP_STUN_GUN, 0.0f);

            Screen.Hud.HideComponentThisFrame(HudComponent.Cash);
            Screen.Hud.HideComponentThisFrame(HudComponent.CashChange);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpCash);
            Screen.Hud.HideComponentThisFrame(HudComponent.MpTagCashFromBank);
            Screen.Hud.HideComponentThisFrame(HudComponent.Saving);

            API.SetTextChatEnabled(false); // Disable default GTA Chat
            API.DisablePlayerVehicleRewards(Game.Player.Handle); // Remove any vehicle weapons
            Game.Player.SetRunSpeedMultThisFrame(1f); // Force the players speed       
        }
    }
}
