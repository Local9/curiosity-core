﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Tools.Client.net.Helpers;
using Curiosity.Tools.Client.net.Menus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Tools.Client.net.Controllers
{
    public class WorldMenu : Menu
    {

        private static readonly Dictionary<CloudHatType, string> CloudHats = new Dictionary<CloudHatType, string>
        {
            [CloudHatType.Altostratus] = "altostratus",
            [CloudHatType.Clear01] = "Clear 01",
            [CloudHatType.Cloudy01] = "Cloudy 01",
            [CloudHatType.HorizonBand1] = "horizonband1",
            [CloudHatType.HorizonBand2] = "horizonband2",
            [CloudHatType.HorizonBand3] = "horizonband3",
            [CloudHatType.Horsey] = "horsey",
            [CloudHatType.Rain] = "RAIN",
            [CloudHatType.Shower] = "shower"
        };

        private CloudHatType _cloudHat;
        /// <summary>
        /// Sets the cloud hat type to whatever you define it to.
        /// </summary>
        public CloudHatType CloudHat
        {
            get => _cloudHat;
            set
            {
                try
                {
                    _cloudHat = value;
                    var val = CloudHats.ContainsKey(_cloudHat) ? CloudHats[_cloudHat] : Enum.GetName(typeof(CloudHatType), _cloudHat) ?? "";
                    Function.Call(Hash._SET_CLOUD_HAT_TRANSITION, val, 1f);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        private bool _isBlackout;
        /// <summary>
        /// Blacks out light emissions, headlights, and interior lights.
        /// </summary>
        public bool Blackout
        {
            get => _isBlackout;
            set
            {
                _isBlackout = value;
                Function.Call(Hash._SET_BLACKOUT, value);
            }
        }

        public float VehicleDensity { get; set; } = 1f;
        public float PedDensity { get; set; } = 1f;

        public WorldMenu(Client client, Menu parent) : base("World Menu", parent)
        {
            Add(new MenuItemSubMenu(client, this, new WeatherMenu(client, this), "Weather"));
            Add(new MenuItemSubMenu(client, this, new CloudHatMenu(client, this), "Cloud Hats"));

            var blackout = new MenuItemCheckbox(client, this, "Toggle Emission")
            {
                IsChecked = () => !Blackout
            };
            blackout.Activate += () =>
            {
                Blackout = !Blackout;
                return Task.FromResult(0);
            };
            Add(blackout);

            var ped = new MenuItemSpinnerF(client, this, "Ped Density", 1f, 0f, 1f, 0.05f, true);
            ped.ValueUpdate += (val) =>
            {
                PedDensity = val;
                return val;
            };
            Add(ped);

            var veh = new MenuItemSpinnerF(client, this, "Vehicle Density", 1f, 0f, 1f, 0.05f, true);
            veh.ValueUpdate += (val) =>
            {
                VehicleDensity = val;
                return val;
            };
            Add(veh);

            client.RegisterTickHandler(OnWorldSettingsTick);
        }

        private async Task OnWorldSettingsTick()
        {
            try
            {
                Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, VehicleDensity);
                Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, VehicleDensity);
                Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, VehicleDensity);

                Function.Call(Hash.SET_PED_DENSITY_MULTIPLIER_THIS_FRAME, PedDensity);
                Function.Call(Hash.SET_SCENARIO_PED_DENSITY_MULTIPLIER_THIS_FRAME, PedDensity);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                await BaseScript.Delay(100);
            }
        }
    }
}
