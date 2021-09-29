using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Racing.Client.Diagnostics;
using Curiosity.Racing.Client.Interface;
using Curiosity.Systems.Library.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Racing.Client
{
    public static class CommonFunctions
    {
        private static Vehicle _previousVehicle;

        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }

        public static async void LockOrUnlockDoors(Vehicle veh, bool lockDoors)
        {
            if (veh != null && veh.Exists())
            {
                for (int i = 0; i < 2; i++)
                {
                    int timer = API.GetGameTimer();
                    while (API.GetGameTimer() - timer < 50)
                    {
                        API.SoundVehicleHornThisFrame(veh.Handle);
                        await Delay(0);
                    }
                    await Delay(50);
                }
                if (lockDoors)
                {
                    Subtitle.Custom("Vehicle doors are now locked.");
                    API.SetVehicleDoorsLockedForAllPlayers(veh.Handle, true);
                }
                else
                {
                    Subtitle.Custom("Vehicle doors are now unlocked.");
                    API.SetVehicleDoorsLockedForAllPlayers(veh.Handle, false);
                }
            }
        }

        public static string GetVehDisplayNameFromModel(string name) => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)API.GetHashKey(name)));
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)API.GetHashKey(modelName));
        public static bool DoesModelExist(uint modelHash) => API.IsModelInCdimage(modelHash);
        public static uint GetVehicleModel(int vehicle) => (uint)API.GetHashKey(API.GetEntityModel(vehicle).ToString());

        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }
        public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }

        public static void QuitSession() => API.NetworkSessionEnd(true, true);

        public static async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.");
            await BaseScript.Delay(5000);
            API.ForceSocialClubUpdate(); // bye bye
        }

        public static async void SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
            if (vehicleName == "custom")
            {
                // Get the result.
                string result = await GetUserInput(windowTitle: "Enter Vehicle Name");
                // If the result was not invalid.
                if (!string.IsNullOrEmpty(result))
                {
                    // Convert it into a model hash.
                    uint model = (uint)API.GetHashKey(result);
                    SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false, vehicleInfo: null);
                }
                // Result was invalid.
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                }
            }
            // Spawn the specified vehicle.
            else
            {
                SpawnVehicle(vehicleHash: (uint)API.GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false,
                    vehicleInfo: null);
            }
        }

        public static async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad, VehicleInfo? vehicleInfo)
        {
            float speed = 0f;
            float rpm = 0f;
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle tmpOldVehicle = GetVehicle();
                speed = API.GetEntitySpeedVector(tmpOldVehicle.Handle, true).Y; // get forward/backward speed only
                rpm = tmpOldVehicle.CurrentRPM;
            }

            int modelClass = API.GetVehicleClassFromName(vehicleHash);

            //if (!VehicleSpawner.allowedCategories[modelClass])
            //{
            //    Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
            //    return;
            //}

            if (!skipLoad)
            {
                bool successFull = await LoadModel(vehicleHash);
                if (!successFull || !API.IsModelAVehicle(vehicleHash))
                {
                    // Vehicle model is invalid.
                    Notify.Error(CommonErrors.InvalidModel);
                    return;
                }
            }

            Logger.Info("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

            // Get the heading & position for where the vehicle should be spawned.
            Vector3 pos = (spawnInside) ? API.GetEntityCoords(Game.PlayerPed.Handle, true) : API.GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
            float heading = API.GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f);

            // If the previous vehicle exists...
            if (_previousVehicle != null)
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (_previousVehicle.Exists() && _previousVehicle.PreviouslyOwnedByPlayer &&
                    (_previousVehicle.Occupants.Count() == 0 || _previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious)
                    {
                        // Delete it.
                        _previousVehicle.PreviouslyOwnedByPlayer = false;
                        API.SetEntityAsMissionEntity(_previousVehicle.Handle, true, true);
                        _previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        //if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_keep_spawned_vehicles_persistent))
                        //{
                        //    // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
                        //    API.SetEntityAsMissionEntity(_previousVehicle.Handle, false, false);
                        //    //_previousVehicle.IsPersistent = false;
                        //    //_previousVehicle.PreviouslyOwnedByPlayer = false;
                        //    //_previousVehicle.MarkAsNoLongerNeeded();
                        //}
                    }
                    _previousVehicle = null;
                }
            }

            if (Game.PlayerPed.IsInVehicle() && replacePrevious)
            {
                if (GetVehicle().Driver == Game.PlayerPed)// && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
                {
                    var tmpveh = GetVehicle();
                    API.SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
                    API.SetEntityAsMissionEntity(tmpveh.Handle, true, true);

                    if (_previousVehicle != null)
                    {
                        if (_previousVehicle.Handle == tmpveh.Handle)
                        {
                            _previousVehicle = null;
                        }
                    }
                    tmpveh.Delete();
                    Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
                }
            }

            if (_previousVehicle != null)
                _previousVehicle.PreviouslyOwnedByPlayer = false;

            if (Game.PlayerPed.IsInVehicle())
                pos = API.GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0, 8f, 0.1f);

            // Create the new vehicle and remove the need to hotwire the car.
            Vehicle vehicle = new Vehicle(API.CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true,
                IsStolen = false,
                IsWanted = false
            };

            Logger.Info($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, created at x:{pos.X} y:{pos.Y} z:{(pos.Z + 1f)} " +
                $"heading:{heading}");

            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;

                // Set the ped into the vehicle.
                new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
                if (vehicle.ClassType == VehicleClass.Helicopters && API.GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 10.0f)
                {
                    API.SetHeliBladesFullSpeed(vehicle.Handle);
                }
                // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
                else
                {
                    vehicle.PlaceOnGround();
                }
            }

            // If mod info about the vehicle was specified, check if it's not null.
            //if (vehicleInfo != null)
            //{
            //// Set the modkit so we can modify the car.
            //SetVehicleModKit(vehicle.Handle, 0);

            //// set the extras
            //foreach (var extra in vehicleInfo.extras)
            //{
            //    if (DoesExtraExist(vehicle.Handle, extra.Key))
            //        vehicle.ToggleExtra(extra.Key, extra.Value);
            //}

            //SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
            //SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
            //if (vehicle.Model.IsBike)
            //{
            //    SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
            //}
            //ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
            //SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
            //ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
            //ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
            //SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

            //SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
            //SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
            //SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

            //SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

            //SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
            //SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

            //SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

            //foreach (var mod in vehicleInfo.mods)
            //{
            //    SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
            //}
            //vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
            //vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
            //vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
            //vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
            //vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);

            //vehicle.CanTiresBurst = !vehicleInfo.bulletProofTires;

            //VehicleOptions._SET_VEHICLE_HEADLIGHTS_COLOR(vehicle, vehicleInfo.headlightColor);
            //}

            // Set the previous vehicle to the new vehicle.
            _previousVehicle = vehicle;
            //vehicle.Speed = speed; // retarded feature that randomly breaks for no fucking reason
            if (!vehicle.Model.IsTrain) // to be extra fucking safe
            {
                // workaround of retarded feature above:
                API.SetVehicleForwardSpeed(vehicle.Handle, speed);
            }
            vehicle.CurrentRPM = rpm;

            // Discard the model.
            API.SetModelAsNoLongerNeeded(vehicleHash);
        }

        public static string[] StringToArray(string inputString)
        {
            return CitizenFX.Core.UI.Screen.StringToArray(inputString);
        }

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (API.IsHudPreferenceSwitchedOn() && !API.IsPlayerSwitchInProgress() && API.IsScreenFadedIn() && !API.IsPauseMenuActive() && !API.IsFrontendFading() && !API.IsPauseMenuRestarting() && !API.IsHudHidden())
            {
                API.SetTextFont(font);
                API.SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    API.SetTextWrap(0f, xPosition);
                }
                API.SetTextJustification((int)justification);
                if (!disableTextOutline) { API.SetTextOutline(); }
                API.BeginTextCommandDisplayText("STRING");
                API.AddTextComponentSubstringPlayerName(text);
                API.EndTextCommandDisplayText(xPosition, yPosition);
            }
        }

        #region Load Model

        private static async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (API.IsModelInCdimage(modelHash))
            {
                // Load the model.
                API.RequestModel(modelHash);
                // Wait until it's loaded.
                while (!API.HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }

        #endregion

        #region GetUserInput
        public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            API.AddTextEntry($"{API.GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength.ToString()} Characters)");

            // Display the input box.
            API.DisplayOnscreenKeyboard(1, $"{API.GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = API.UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return API.GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
                }
            }
        }
        #endregion

        #region Animation Loading
        public static async Task LoadAnimationDict(string dict)
        {
            while (!API.HasAnimDictLoaded(dict))
            {
                API.RequestAnimDict(dict);
                await BaseScript.Delay(5);
            }
        }
        #endregion
    }
}
