﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace RS9000
{
    internal class Script : BaseScript
    {
        private readonly Radar Radar;
        private readonly Controller controller;

        public string ResourceName { get; }

        public Config Config { get; }

        private bool IsDisplayingKeyboard { get; set; }

        private bool sentInit = false;

        public Script()
        {
            try
            {
                ResourceName = API.GetCurrentResourceName();
                foreach (char c in ResourceName)
                {
                    if (char.IsUpper(c))
                    {
                        throw new Exception("Resource name cannot contain uppercase characters");
                    }
                }

                string configData = API.LoadResourceFile(ResourceName, "config.json");

                if (string.IsNullOrEmpty(configData))
                {
                    Debug.WriteLine($"{ResourceName} -> Unable to load config file");
                }
                else
                {
                    Config = Config.Base;
                    JsonConvert.PopulateObject(configData, Config);
                    Config.Validate();

                    Radar = new Radar(this);
                    controller = new Controller(this, Radar);

                    RegisterEventHandler("rs9000:ToggleRadar", new Action(OnToggleRadar));

                    Tick += Update;
                    Tick += CheckInputs;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ResourceName} -> {ex}");
            }
        }

        public void RegisterEventHandler(string eventName, Delegate callback)
        {
            EventHandlers[eventName] += callback;
        }

        public void RegisterNUICallback(string msg, Delegate callback)
        {
            try
            {
                API.RegisterNuiCallbackType(msg);
                EventHandlers[$"__cfx_nui:{msg}"] += new Func<ExpandoObject, CallbackDelegate, Task>(async (body, result) =>
                {
                    object rv = callback?.DynamicInvoke(body, result);
                    if (rv != null && rv is Task t)
                    {
                        await t;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ResourceName} -> {ex}");
            }
        }

        public static Vehicle GetVehicleDriving(Ped ped)
        {
            Vehicle v = ped.CurrentVehicle;
            bool driving = ped.SeatIndex == VehicleSeat.Driver;

            if (v == null || !driving || v.ClassType != VehicleClass.Emergency)
            {
                return null;
            }

            return v;
        }

        private bool InEmergencyVehicle => GetVehicleDriving(Game.PlayerPed) != null;

        private async Task CheckInputs()
        {
            if (API.IsPauseMenuActive())
            {
                if (Radar.IsDisplayed)
                {
                    Radar.IsDisplayed = false;
                }
                return;
            }

            bool inEmergencyVehicle = InEmergencyVehicle;

            if (Radar.IsDisplayed && !inEmergencyVehicle)
            {
                Radar.IsDisplayed = false;
            }
            else if (inEmergencyVehicle && !Radar.IsDisplayed && Radar.IsEnabled)
            {
                Radar.IsDisplayed = true;
            }

            //if (ControlPressed(Config.Controls.OpenControlPanel) && inEmergencyVehicle)
            //{
            //    controller.Visible = !controller.Visible;
            //}

            if (ControlPressed(Config.Controls.ResetLock) && inEmergencyVehicle)
            {
                Radar.ResetFast();
            }

            await Task.FromResult(0);
        }

        private void OnToggleRadar()
        {
            controller.Visible = !controller.Visible;
        }

        private bool ControlPressed(ControlConfig config)
        {
            bool modifier = config.Modifier.HasValue ? Game.IsControlPressed(0, config.Modifier.Value) : true;
            bool control = config.Control.HasValue ? Game.IsControlJustPressed(0, config.Control.Value) : false;
            return modifier && control;
        }

        private async Task Update()
        {
            if (!sentInit)
            {
                SendMessage(MessageType.Initialize, new
                {
                    resourceName = ResourceName,
                    plateReader = Config.PlateReader,
                });

                Radar.FastLimit = Radar.ConvertSpeedToMeters(Config.Units, Config.FastLimit);
                Radar.ShouldBeep = Config.Beep;

                sentInit = true;
            }

            await Delay(10);

            Radar.Update();
        }

        public static void SendMessage(MessageType type, object data)
        {
            string json = JObject.FromObject(new { type, data }).ToString();
            API.SendNuiMessage(json);
        }
    }
}
