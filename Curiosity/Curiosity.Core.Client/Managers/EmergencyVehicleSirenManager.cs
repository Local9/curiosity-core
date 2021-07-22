using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface.Menus;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class EmergencyVehicleSirenManager : Manager<EmergencyVehicleSirenManager>
    {
        bool _lightsActive = false;
        bool _sirenActive = false;

        /*
         * Add a config file for vehicle hashes and sirens
         * Load config into the client
         * Only add sirens to those vehicles that exist
         * 
         * */

        public override void Begin()
        {

        }

        public async void EnableSirenManager()
        {
            Logger.Debug($"Started Siren Manager");

            Instance.AttachTickHandler(OnSirenDisableControls);
            Instance.AttachTickHandler(OnSirenControlInput);

            while (Cache.PlayerPed.IsInVehicle())
            {
                await BaseScript.Delay(250);
            }

            Instance.DetachTickHandler(OnSirenDisableControls);
            Instance.DetachTickHandler(OnSirenControlInput);
        }

        private async Task OnSirenDisableControls() // could drop this
        {
            if (!Cache.PlayerPed.IsInVehicle()) return;

            if (GetVehicleClass(Cache.PlayerPed.CurrentVehicle.Handle) == (int)VehicleClass.Emergency)
            {
                SetVehicleRadioEnabled(Cache.PlayerPed.CurrentVehicle.Handle, false);

                // different control group
                DisableControlAction(0, (int)Control.VehicleHorn, true);
            }
        }

        private async Task OnSirenControlInput()
        {
            if (InteractionMenu.MenuPool.IsAnyMenuOpen())
            {
                await BaseScript.Delay(100);
            }

            ToggleLights();
            ToggleSiren();
        }

        private async void ToggleLights()
        {
            Control lightStageControl = Control.VehicleRadioWheel;
            Game.DisableControlThisFrame(0, lightStageControl);
            Game.DisableControlThisFrame(2, Control.CharacterWheel);

            if (((Game.IsDisabledControlJustPressed(0, lightStageControl) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.GamePad))
                && !_lightsActive)
            {
                _lightsActive = true;
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, true, true);

                await BaseScript.Delay(100);
            }

            if (((Game.IsDisabledControlJustPressed(0, lightStageControl) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, Control.CharacterWheel) && Game.CurrentInputMode == InputMode.GamePad))
                && _lightsActive)
            {
                _lightsActive = false;
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, false, true);
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "STOP", true);

                await BaseScript.Delay(100);
            }
        }

        private async void ToggleSiren()
        {
            Control sirenToggle = Control.CharacterWheel;
            Control sirenToggleController = Control.HUDSpecial;

            if (((Game.IsDisabledControlJustPressed(0, sirenToggle) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, sirenToggleController) && Game.CurrentInputMode == InputMode.GamePad))
                && !_sirenActive && _lightsActive)
            {
                _sirenActive = true;
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_STATE, true, true);
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "VEHICLES_HORNS_SIREN_1", true); // sounds from a file

                await BaseScript.Delay(100);
            }

            if (((Game.IsDisabledControlJustPressed(0, sirenToggle) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, sirenToggleController) && Game.CurrentInputMode == InputMode.GamePad))
                && _sirenActive)
            {
                _sirenActive = false;
                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);

                await BaseScript.Delay(100);
            }
        }

        [TickHandler]
        private async Task OnCheckVehicleSirenStates()
        {
            Vehicle[] vehicles = World.GetAllVehicles();
            for (int i = 0; i < vehicles.Length; i++)
            {
                Vehicle vehicle = vehicles[i];
                if (vehicle?.Exists() ?? false)
                {
                    if (vehicle.ClassType != VehicleClass.Emergency) continue;

                    if (!(vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false)) continue;

                    bool sirenSetup = vehicle.State.Get("siren:setup") ?? false;
                    bool lightSetup = vehicle.State.Get("light:setup") ?? false;
                    string lastSoundFile = vehicle.State.Get("siren:lastSound");

                    if (vehicle.State.Get(StateBagKey.VEH_SIREN_LIGHTS) ?? false)
                    {
                        if (!lightSetup)
                        {
                            vehicle.State.Set("light:setup", true, false);
                            vehicle.IsSirenActive = true;
                            vehicle.IsSirenSilent = true;
                            int toggle = 1;
                            SetSirenWithNoDriver(vehicle.Handle, ref toggle);
                        }
                    }
                    
                    if (!(vehicle.State.Get(StateBagKey.VEH_SIREN_LIGHTS) ?? false))
                    {
                        if (lightSetup)
                        {
                            vehicle.State.Set("light:setup", false, false);
                            vehicle.IsSirenActive = false;
                            int toggle = 0;
                            SetSirenWithNoDriver(vehicle.Handle, ref toggle);
                            StopSound(vehicle.Handle);
                            ReleaseSoundId(vehicle.Handle);
                        }
                    }

                    if (vehicle.State.Get(StateBagKey.VEH_SIREN_STATE) ?? false && lightSetup)
                    {
                        string soundToPlay = vehicle.State.Get(StateBagKey.VEH_SIREN_SOUND);

                        if (!sirenSetup)
                        {
                            vehicle.State.Set("siren:setup", true, false);    

                            if (string.IsNullOrEmpty(soundToPlay))
                                soundToPlay = "VEHICLES_HORNS_SIREN_1";

                            vehicle.State.Set("siren:lastSound", soundToPlay, false);

                            PlaySoundFromEntity(vehicle.Handle, soundToPlay, vehicle.Handle, "PLAYER_SIRENS", false, 0);
                            vehicle.IsSirenSilent = false;
                        }

                        if (lastSoundFile != soundToPlay)
                        {
                            vehicle.State.Set("siren:lastSound", soundToPlay, false);
                            PlaySoundFromEntity(vehicle.Handle, soundToPlay, vehicle.Handle, "PLAYER_SIRENS", false, 0);
                        }
                    }

                    if (!(vehicle.State.Get(StateBagKey.VEH_SIREN_STATE) ?? false))
                    {
                        if (sirenSetup)
                        {
                            vehicle.State.Set("siren:setup", false, false);

                            vehicle.IsSirenSilent = true;
                            PlaySoundFromEntity(vehicle.Handle, "STOP", vehicle.Handle, "PLAYER_SIRENS", false, 0);
                        }
                    }
                }
            }
        }
    }
}
