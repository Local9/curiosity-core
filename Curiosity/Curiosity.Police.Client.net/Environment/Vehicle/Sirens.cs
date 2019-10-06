using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Police.Client.net.Environment.Vehicle
{
    static class Sirens
    {
        static Client client = Client.GetInstance();

        static List<string> SirenModes = new List<string>()
        {
            "", // No Sirens, just lights
            "VEHICLES_HORNS_SIREN_1",
            "VEHICLES_HORNS_SIREN_2"
        };

        static string CurrentSirenPreset = SirenModes[0];

        static Dictionary<int, int> SirenSoundIds = new Dictionary<int, int>();

        static bool SirenActive = false;
        static bool LightsActive = false;
        static bool IsMenuOpen = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnTick);
            client.RegisterEventHandler("curiosity:Player:Vehicle:Siren:SoundEvent", new Action<string>(OnReceivedSourceEvent));
            client.RegisterEventHandler("curiosity:Client:Menu:IsOpened", new Action<bool>(OnMenuStateChange));
            API.DecorRegister("Vehicle.SirensInstalled", 2);
        }

        static private void OnMenuStateChange(bool state)
        {
            IsMenuOpen = state;
        }

        static private async Task HideHudComponent()
        {
            while (LightsActive)
            {
                API.HideHudComponentThisFrame(1); // Wanted Stars
                await Client.Delay(0);

                if (Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle.Exists())
                    {
                        if (Client.CurrentVehicle.IsDead)
                        {
                            API.SetFakeWantedLevel(0);
                            LightsActive = false;
                            break;
                        }
                    }
                }
            }
            client.DeregisterTickHandler(HideHudComponent);
            await Client.Delay(0);
        }

        static private async Task OnTick()
        {

            if (IsMenuOpen)
            {
                await BaseScript.Delay(100);
                return;
            }

            if (Game.PlayerPed.IsInVehicle())
            {
                if (API.GetVehicleClass(Game.PlayerPed.CurrentVehicle.Handle) == (int)VehicleClass.Emergency)
                {
                    API.DisableControlAction(0, 86, true);
                    API.DisableControlAction(0, (int)Control.VehicleCinCam, true);
                    API.DisableControlAction(0, (int)Control.VehicleLookBehind, true);
                    API.DisableControlAction(0, (int)Control.LookBehind, true);
                }
            }

            if (!SirenActive && Game.PlayerPed.IsInVehicle()
                && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed
                && (((Game.PlayerPed.CurrentVehicle.Model.IsCar
                    || Game.PlayerPed.CurrentVehicle.Model.IsBike
                    || Game.PlayerPed.CurrentVehicle.Model.IsBoat)
                    && API.DecorGetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled"))
                    || Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency))
            {
                if (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) || API.IsDisabledControlPressed(0, (int)Control.VehicleCinCam))
                {
                    SirenActive = true;
                    SendSoundEvent("SIRENS_AIRHORN");
                    while ((ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) || API.IsDisabledControlPressed(0, (int)Control.VehicleCinCam)) && Game.PlayerPed.IsInVehicle())
                    {
                        API.DisableControlAction(0, (int)Control.VehicleCinCam, true);
                        await BaseScript.Delay(0);
                    }
                    StopSound();
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) || API.IsDisabledControlPressed(16, (int)Control.VehicleLookBehind))
                {
                    SirenActive = true;
                    SendSoundEvent("VEHICLES_HORNS_POLICE_WARNING");
                    while ((ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) || API.IsDisabledControlPressed(16, (int)Control.VehicleLookBehind)) && Game.PlayerPed.IsInVehicle())
                    {
                        API.DisableControlAction(0, (int)Control.VehicleLookBehind, true);
                        await BaseScript.Delay(0);
                    }
                    StopSound();
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlJustPressed(Control.SpecialAbilitySecondary, true, ControlModifier.Any))
                {
                    SirenActive = true;
                    Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);
                    await BaseScript.Delay(700);
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlJustPressed(Control.ThrowGrenade) || API.IsControlJustPressed(7, (int)Control.SniperZoomOutSecondary)) // Preset on/off
                {
                    LightsActive = true;
                    client.RegisterTickHandler(HideHudComponent);

                    API.SetFakeWantedLevel(1);
                    Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, Game.PlayerPed.CurrentVehicle.Handle, true);
                    Function.Call(Hash.SET_VEHICLE_SIREN, Game.PlayerPed.CurrentVehicle.Handle, true);
                    Function.Call(Hash.SET_SIREN_WITH_NO_DRIVER, Game.PlayerPed.CurrentVehicle.Handle, true);

                    SirenActive = true;
                    PlayCurrentPresetSound();

                    while (Game.PlayerPed.IsInVehicle())
                    {
                        if (API.GetVehicleClass(Game.PlayerPed.CurrentVehicle.Handle) == (int)VehicleClass.Emergency)
                        {
                            API.DisableControlAction(0, 86, true);
                            API.DisableControlAction(0, (int)Control.VehicleCinCam, true);
                            API.DisableControlAction(0, (int)Control.VehicleLookBehind, true);
                        }

                        await BaseScript.Delay(0);
                        if (ControlHelper.IsControlJustPressed(Control.ThrowGrenade) || API.IsControlJustPressed(7, (int)Control.SniperZoomOutSecondary))
                        {
                            API.SetFakeWantedLevel(0);
                            LightsActive = false;
                            break;
                        }
                        else if (ControlHelper.IsControlJustPressed(Control.MpTextChatTeam) || API.IsControlJustPressed(16, (int)Control.VehicleFlyUnderCarriage)) // Cycle presets
                        {
                            StopSound();
                            CurrentSirenPreset = SirenModes[(SirenModes.IndexOf(CurrentSirenPreset) + 1) % SirenModes.Count];
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) || API.IsDisabledControlPressed(0, (int)Control.VehicleCinCam))
                        {
                            StopSound();
                            SendSoundEvent("SIRENS_AIRHORN");
                            while ((ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) || API.IsDisabledControlPressed(0, (int)Control.VehicleCinCam)) && Game.PlayerPed.IsInVehicle())
                            {
                                API.DisableControlAction(0, (int)Control.VehicleCinCam, true);
                                await BaseScript.Delay(0);
                            }
                            StopSound();
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) || API.IsDisabledControlPressed(16, (int)Control.VehicleLookBehind))
                        {
                            StopSound();
                            SendSoundEvent("VEHICLES_HORNS_POLICE_WARNING");
                            string internalPreset = CurrentSirenPreset;
                            while ((ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) || API.IsDisabledControlPressed(16, (int)Control.VehicleLookBehind)) && Game.PlayerPed.IsInVehicle())
                            {
                                API.DisableControlAction(0, (int)Control.VehicleLookBehind, true);
                                await BaseScript.Delay(0);
                            }
                            CurrentSirenPreset = internalPreset;
                            StopSound();
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.SpecialAbilitySecondary, true, ControlModifier.Any))
                        {
                            StopSound();
                            Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);
                            await BaseScript.Delay(700);
                            PlayCurrentPresetSound();
                        }
                    }

                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, Game.PlayerPed.CurrentVehicle.Handle, false);
                        Function.Call(Hash.SET_VEHICLE_SIREN, Game.PlayerPed.CurrentVehicle.Handle, false);
                        Function.Call(Hash.SET_SIREN_WITH_NO_DRIVER, Game.PlayerPed.CurrentVehicle.Handle, false);
                    }
                    StopSound();
                    SirenActive = false;
                }
            }
            else if (!Game.PlayerPed.IsInVehicle())
            {
                SirenActive = false;
            }
        }

        static void OnReceivedSourceEvent(string serializedSoundEvent)
        {
            try
            {
                SoundEventModel SoundEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<SoundEventModel>(serializedSoundEvent);
                if (SoundEvent.SoundName == "STOP" && SirenSoundIds.ContainsKey(SoundEvent.PlayerServerId) && SirenSoundIds[SoundEvent.PlayerServerId] != -1)
                {
                    Function.Call(Hash.STOP_SOUND, SirenSoundIds[SoundEvent.PlayerServerId]);
                    Function.Call(Hash.RELEASE_SOUND_ID, SirenSoundIds[SoundEvent.PlayerServerId]);
                    SirenSoundIds[SoundEvent.PlayerServerId] = -1;
                }
                else
                {
                    if (SirenSoundIds.ContainsKey(SoundEvent.PlayerServerId) && SirenSoundIds[SoundEvent.PlayerServerId] != -1)
                    {
                        Function.Call(Hash.STOP_SOUND, SirenSoundIds[SoundEvent.PlayerServerId]);
                        Function.Call(Hash.RELEASE_SOUND_ID, SirenSoundIds[SoundEvent.PlayerServerId]);
                    }
                    SirenSoundIds[SoundEvent.PlayerServerId] = Function.Call<int>(Hash.GET_SOUND_ID);
                    Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, SirenSoundIds[SoundEvent.PlayerServerId], SoundEvent.SoundName, Client.players[SoundEvent.PlayerServerId].Character.CurrentVehicle.Handle, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                if (Classes.Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    // Log.Error($"Siren event error: {ex.Message}");
                }
            }
        }

        static void SendSoundEvent(string sound)
        {
            string serializedSoundEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new SoundEventModel { SoundName = sound, PlayerServerId = Game.Player.ServerId });
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Siren:SoundEvent", serializedSoundEvent));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }

        static public void PlaySound(int sourceServerId, string sound)
        {
            SirenSoundIds[Game.Player.ServerId] = Function.Call<int>(Hash.GET_SOUND_ID);
            Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, SirenSoundIds[Game.Player.ServerId], sound, Game.PlayerPed.CurrentVehicle.Handle, 0, 0, 0);
        }

        static void PlayCurrentPresetSound()
        {
            SendSoundEvent(CurrentSirenPreset);
        }

        static void StopSound()
        {
            SendSoundEvent("STOP");
        }
    }
}
