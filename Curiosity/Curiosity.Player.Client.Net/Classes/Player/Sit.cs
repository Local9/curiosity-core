using CitizenFX.Core;
using Curiosity.Client.net.Helpers.Dictionary;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Player
{
    class Sit
    {
        static Client client = Client.GetInstance();

        static float MaxDistance = 1.5f;

        static bool IsSitting = false;
        static Vector3 lastPos = new Vector3();
        static Vector3 seatCoords = new Vector3();
        static string currentSitId;
        static string currentScenario;
        static string customId;
        static string action = string.Empty;
        static Entity currentProp;
        static string currentPropName;

        public static void Init()
        {
            PlayerInteractables.SetupInteractables();
            PlayerInteractables.SetupSitableItems();

            client.RegisterTickHandler(OnTick);
            client.RegisterTickHandler(DisplayButtonInstructionCheck);

            client.RegisterEventHandler("curiosity:Player:Environment:CanTakeSeat", new Action<string, bool>(PlayerCanSit));
        }

        static async Task DisplayButtonInstructionCheck()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(action))
                {
                    SetTextComponentFormat("STRING");
                    AddTextComponentString($" Press ~INPUT_PICKUP~ to {action}.");
                    DisplayHelpTextFromStringLabel(0, false, true, -1);
                }
                await Client.Delay(0);
                await Task.FromResult(0);
            }
        }

        static async Task OnTick()
        {
            try
            {
                int playerPed = Game.PlayerPed.Handle;

                if (IsSitting && IsPedUsingScenario(playerPed, currentScenario))
                {
                    if (ControlHelper.IsControlJustPressed(Control.Pickup))
                    {
                        GetUp();
                    }
                }

                currentProp = Helpers.WorldProbe.GetPropEntityInFrontOfPlayer(MaxDistance);

                if (currentProp != null)
                {

                    currentPropName = $"{(ObjectHash)currentProp.Model.NativeValue}";

                    if (PlayerInteractables.InteractableContains(currentPropName))
                    {
                        action = "Sit";

                        if (ControlHelper.IsControlJustPressed(Control.Pickup) && !IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
                        {
                            PlayerSit(currentProp, currentPropName);
                        }
                    }
                    else
                    {
                        action = string.Empty;
                    }
                }
                else
                {
                    action = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            await Task.FromResult(0);
        }

        static void PlayerSit(Entity prop, string modelName)
        {
            seatCoords = prop.Position;
            customId = $"{seatCoords.X}{seatCoords.Y}{seatCoords.Z}";
            currentProp = prop;
            Client.TriggerServerEvent("curiosity:Server:Environment:CanTakeSeat", customId);
        }

        static void PlayerCanSit(string customId, bool seatIsTaken)
        {
            try
            {
                if (seatIsTaken)
                {
                    currentProp = null;
                }
                else
                {
                    lastPos = Game.PlayerPed.Position;
                    currentSitId = customId;
                    Client.TriggerServerEvent("curiosity:Server:Environment:TakeSeat", customId);
                    FreezeEntityPosition(currentProp.Handle, true);
                    Helpers.Dictionary.Entity.InteractableSetting setting = PlayerInteractables.GetInteractableSetting(currentPropName);
                    currentScenario = setting.Scenario;
                    TaskStartScenarioAtPosition(Game.PlayerPed.Handle, currentScenario, seatCoords.X, seatCoords.Y, seatCoords.Z - setting.VerticalOffset, currentProp.Heading + 180.0f, 0, true, true);
                    IsSitting = true;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        static void GetUp()
        {
            Client.TriggerServerEvent("curiosity:Server:Environment:LeaveSeat", customId);
            Client.Delay(0);

            int playerPed = Game.PlayerPed.Handle;
            ClearPedTasks(playerPed);
            IsSitting = false;
            Game.PlayerPed.Position = lastPos;
            FreezeEntityPosition(playerPed, false);
            FreezeEntityPosition(currentProp.Handle, false);
            currentProp = null;
            currentScenario = string.Empty;
        }
    }
}
