using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net.Classes.Environment
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Command:SpawnCar", new Action<string, string>(SpawnCar));

            RegisterCommand("sirens", new Action<int, List<object>, string>(SirensCommand), false);
            RegisterCommand("vf", new Action<int, List<object>, string>(OnVehicleForceSpawn), false);
        }

        static async void OnVehicleForceSpawn(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

                string prop = "xs_prop_arena_drone_01";

                Model model = new Model(prop);
                await model.Request(10000);
                await World.CreateProp(model, Game.PlayerPed.Position + new Vector3(0f, 5f, 0f), true, true);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }


        static void SirensCommand(int playerHandle, List<object> arguments, string raw)
        {
            ShowSirenKeys();
        }

        static public void ShowSirenKeys()
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Sirens", "How To Keyboard...", "Enable Sirens: ~b~G/L-ALT~n~~s~Change Siren: ~b~Y~n~~s~Horn: ~b~CTRL~n~~s~Blip Siren: ~b~B", 2);
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Sirens", "How To XBOX Controls...", "Enable Sirens: ~b~DPAD Down~n~~s~Change Siren: ~b~L3~n~~s~Horn: ~b~B~n~~s~Blip Siren: ~b~R3", 2);
        }

        static async void SpawnCar(string car, string numberPlate)
        {
            try
            {
                if (!Player.PlayerInformation.IsStaff()) return;
                if (string.IsNullOrEmpty(car)) return;

                Model model = null;
                var enumName = Enum.GetNames(typeof(VehicleHash)).FirstOrDefault(s => s.ToLower().StartsWith(car.ToLower())) ?? "";

                var modelName = "";

                if (int.TryParse(car, out var hash))
                {
                    model = new Model(hash);
                    modelName = $"{hash}";
                }
                else if (!string.IsNullOrEmpty(enumName))
                {
                    var found = false;
                    foreach (VehicleHash p in Enum.GetValues(typeof(VehicleHash)))
                    {
                        if (!(Enum.GetName(typeof(VehicleHash), p)?.Equals(enumName, StringComparison.InvariantCultureIgnoreCase) ??
                              false))
                        {
                            continue;
                        }

                        model = new Model(p);
                        modelName = enumName;
                        found = true;

                        if (Classes.Player.PlayerInformation.IsDeveloper())
                        {
                            Screen.ShowNotification($"~r~Info~s~:~n~Model Valid: {model.IsValid}~n~Model: {modelName}");
                        }

                        break;
                    }

                    if (!model.IsValid)
                    {
                        Screen.ShowNotification($"~r~ERROR~s~: Could not model {car}");
                        return;
                    }

                    if (!found)
                    {
                        Screen.ShowNotification($"~r~ERROR~s~: Could not load model {car}");
                        return;
                    }
                }
                else
                {
                    model = new Model(car);
                    modelName = car;
                }

                if (!await Spawn.SpawnVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading, false, true, numberPlate))
                {
                    Client.TriggerEvent("", 1, "Curiosity", "Vehicle Error", $"Could not load model {modelName}", 2);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
