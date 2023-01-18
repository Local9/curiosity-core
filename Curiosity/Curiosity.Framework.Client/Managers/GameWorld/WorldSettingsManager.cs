using CitizenFX.Core.UI;

namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldSettingsManager : Manager<WorldSettingsManager>
    {
        private int MP_STUN_GUN;
        private int blipMainPlayer;
        
        public override void Begin()
        {
            SetMinimapBlockWaypoint(false);
            
            ReserveNetworkMissionVehicles(3);
            ReserveNetworkMissionPeds(4);

            SetVehiclePopulationBudget(3);

            if (DoesScenarioGroupExist("MP_POLICE"))
            {
                if (IsScenarioGroupEnabled("MP_POLICE"))
                {
                    SetScenarioGroupEnabled("MP_POLICE", false);
                }
            }

            MP_STUN_GUN = GetHashKey("WEAPON_STUNGUN_MP");

            SetAmbientPedsDropMoney(false);
            SetAiWeaponDamageModifier(0.72f);
            SetAiMeleeWeaponDamageModifier(0.72f);
            // No peds in trevor bros house!
            AddScenarioBlockingArea(-1154.9647f, -1520.9827f, 9.132731f, -1158.9647f, -1524.9827f, 11.632731f, false, true, true, true);

            blipMainPlayer = GetMainPlayerBlipId();
            if (DoesBlipExist(blipMainPlayer))
            {
                SetBlipNameFromTextFile(blipMainPlayer, "BLIP_PLAYER");
            }

            AddNavmeshBlockingObject(956.039f, 18.564f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(948.664f, 23.339f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(950.664f, 10.139f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(947.589f, 5.239f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(940.114f, 9.714f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(943.114f, 14.614f, 121.077f, 4.275f, 7f, 3.55f, 57.615f, false, 7);
            AddNavmeshBlockingObject(910.15f, 47.35f, 111.375f, 0.85f, 0.85f, 3.55f, 0f, false, 7);
            AddNavmeshBlockingObject(918.625f, 52f, 111.375f, 0.85f, 0.85f, 3.55f, 0f, false, 7);
            AddNavmeshBlockingObject(924.4f, 51.6f, 111.375f, 0.85f, 0.85f, 3.55f, 0f, false, 7);
            AddNavmeshBlockingObject(918.3f, 41.525f, 111.375f, 0.85f, 0.85f, 3.55f, 0f, false, 7);

            SetVehicleModelIsSuppressed((uint)GetHashKey("jet"), true);
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

            Game.Player.SetRunSpeedMultThisFrame(1f); // Speed hack to death            
        }
    }
}
