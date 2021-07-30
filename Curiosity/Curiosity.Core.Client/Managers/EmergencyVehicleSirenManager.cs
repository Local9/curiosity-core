using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface.Menus;
using Curiosity.Systems.Library.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class EmergencyVehicleSirenManager : Manager<EmergencyVehicleSirenManager>
    {
        bool _lightsActive = false;
        bool _sirenActive = false;

        Dictionary<int, Siren> sirens = new Dictionary<int, Siren>();
        Siren activeConfig;

        /*
         * Add a config file for vehicle hashes and sirens
         * Load config into the client
         * Only add sirens to those vehicles that exist
         * 
         * */

        public override async void Begin()
        {
            await Session.Loading();

            CreateSirenDictionary();
        }

        private void CreateSirenDictionary()
        {
            List<Siren> sirensConfig = GetConfig();

            for (int i = 0; i < sirensConfig.Count; i++)
            {
                Siren siren = sirensConfig[i];
                int vehicleHash = GetHashKey(siren.Hash);

                Model model = new Model(vehicleHash);

                if (!model.IsValid) continue;

                if (!sirens.ContainsKey(vehicleHash))
                {
                    sirens.Add(vehicleHash, siren);
                }
            }

            Logger.Debug($"Added {sirens.Count} sirens from config");
        }

        private List<Siren> GetConfig()
        {
            List<Siren> config = new List<Siren>();

            string jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/sirens.json"); // Fuck you VS2019 UTF8 BOM

            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
                    Logger.Error($"sirens.json file is empty or does not exist, please fix this");
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<Siren>>(jsonFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Siren JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public async void EnableSirenManager(Vehicle vehicle)
        {
            if (!sirens.ContainsKey(vehicle.Model.Hash)) return;

            activeConfig = sirens[vehicle.Model.Hash];

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

            SetVehicleRadioEnabled(Cache.PlayerPed.CurrentVehicle.Handle, false);
            DisableControlAction(0, (int)Control.VehicleHorn, true);
        }

        private async Task OnSirenControlInput()
        {
            if (InteractionMenu.MenuPool.IsAnyMenuOpen())
            {
                await BaseScript.Delay(100);
            }

            ToggleLights();
            ToggleSiren();
            ToggleSirenSoundTone1();
            ToggleSirenSoundTone2();
        }

        private async void ToggleSirenSoundTone1()
        {
            Control sirenToneControl = Control.SelectWeaponUnarmed;
            Game.DisableControlThisFrame(0, sirenToneControl);
            Game.DisableControlThisFrame(2, Control.Reload);

            if (((Game.IsDisabledControlJustPressed(0, sirenToneControl) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, Control.Reload) && Game.CurrentInputMode == InputMode.GamePad))
                && _lightsActive)
            {
                if (activeConfig.Sirens.Count == 0)
                {
                    NotificationManger.GetModule().Error($"There are no siren sounds for this vehicle!");
                    return;
                }

                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, activeConfig.Sirens[0], true); // sounds from a file
                await BaseScript.Delay(100);
            }
        }

        private async void ToggleSirenSoundTone2()
        {
            Control sirenToneControl = Control.SelectWeaponMelee;
            Game.DisableControlThisFrame(0, sirenToneControl);
            Game.DisableControlThisFrame(2, Control.SniperZoomInSecondary);

            if (((Game.IsDisabledControlJustPressed(0, sirenToneControl) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                (Game.IsControlJustPressed(2, Control.SniperZoomInSecondary) && Game.CurrentInputMode == InputMode.GamePad))
                && _lightsActive)
            {
                if (activeConfig.Sirens.Count == 0)
                {
                    NotificationManger.GetModule().Error($"There are no siren sounds for this vehicle!");
                    return;
                }

                Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, activeConfig.Sirens[1], true); // sounds from a file
                await BaseScript.Delay(100);
            }
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

                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                vehicle.State.Set(StateBagKey.VEH_SIREN_LIGHTS, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_STATE, false, true);
                vehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, "STOP", true);

                StopSound(vehicle.NetworkId);
                ReleaseSoundId(vehicle.NetworkId);

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
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                vehicle.State.Set(StateBagKey.VEH_SIREN_STATE, true, true);

                if (activeConfig.Sirens.Count == 0)
                {
                    NotificationManger.GetModule().Error($"There are no siren sounds for this vehicle!");
                }
                else
                {
                    string lastSound = vehicle.State.Get("siren:lastSound");

                    if (string.IsNullOrEmpty(lastSound))
                        lastSound = activeConfig.Sirens[0];

                    Game.PlayerPed.CurrentVehicle.State.Set(StateBagKey.VEH_SIREN_SOUND, lastSound, true); // sounds from a file
                }

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

        [TickHandler(SessionWait = true)]
        private async Task OnCheckVehicleSirenStates()
        {
            await BaseScript.Delay(500);

            Vehicle[] vehicles = World.GetAllVehicles().Where(x => Game.PlayerPed.Position.Distance(x.Position) < 100f).ToArray(); // figure out better filtering

            for (int i = 0; i < vehicles.Length; i++)
            {
                Vehicle vehicle = vehicles[i];

                if (!(vehicle?.Exists() ?? false)) continue;

                if (!sirens.ContainsKey(vehicle.Model.Hash)) continue;

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
                        StopSound(vehicle.NetworkId);
                        ReleaseSoundId(vehicle.NetworkId);
                    }
                }

                if (vehicle.State.Get(StateBagKey.VEH_SIREN_STATE) ?? false && lightSetup)
                {
                    string soundToPlay = vehicle.State.Get(StateBagKey.VEH_SIREN_SOUND);

                    await BaseScript.Delay(0);

                    if (!sirenSetup)
                    {
                        vehicle.State.Set("siren:setup", true, false);
                        vehicle.State.Set("siren:lastSound", soundToPlay, false);

                        PlaySoundFromEntity(vehicle.NetworkId, soundToPlay, vehicle.Handle, string.Empty, false, 0);
                        vehicle.IsSirenSilent = true;
                    }

                    if (lastSoundFile != soundToPlay)
                    {
                        StopSound(vehicle.NetworkId);
                        ReleaseSoundId(vehicle.NetworkId);

                        vehicle.State.Set("siren:lastSound", soundToPlay, false);
                        PlaySoundFromEntity(vehicle.NetworkId, soundToPlay, vehicle.Handle, string.Empty, false, 0);
                        Logger.Debug($"Changing Siren {lastSoundFile} -> {soundToPlay}");
                    }
                }

                if (!(vehicle.State.Get(StateBagKey.VEH_SIREN_STATE) ?? false))
                {
                    if (sirenSetup)
                    {
                        vehicle.State.Set("siren:setup", false, false);

                        vehicle.IsSirenSilent = true;
                        PlaySoundFromEntity(vehicle.NetworkId, "STOP", vehicle.Handle, string.Empty, false, 0);
                    }
                }
            }
        }
    }
}
