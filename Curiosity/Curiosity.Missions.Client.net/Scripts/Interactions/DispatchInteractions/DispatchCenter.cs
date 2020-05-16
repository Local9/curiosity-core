using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.MissionPeds;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.DispatchInteractions
{
    class DispatchCenter
    {
        static public void InteractionArrestPed(InteractivePed interactivePed)
        {
            Client.TriggerEvent("curiosity:interaction:arrest", interactivePed.Handle, true);
        }

        static public async void InteractionRunPedIdentification(InteractivePed interactivePed)
        {
            if (!interactivePed.HasProvidedId)
            {
                Screen.ShowNotification("~r~You have to ask for the ~o~Driver's ID~r~ first!");
                return;
            }

            Helpers.Animations.AnimationRadio();
            Wrappers.Helpers.ShowNotification("Dispatch", $"Running ~o~{interactivePed.Name}", string.Empty);
            await Client.Delay(2000);

            if (!interactivePed.HasIdentifcationBeenRan)
            {
                interactivePed.Set(Client.DECOR_INTERACTION_RAN_ID, true);
            }

            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Name: ~y~{interactivePed.Name}~w~\nGender: ~b~{interactivePed.Ped.Gender}~w~\nDOB: ~b~{interactivePed.DateOfBirth}");
            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Citations: {interactivePed.NumberOfCitations}\n~w~Flags: {interactivePed.Offence}");
        }
        static public async void InteractionRunPedVehicle(InteractivePed interactivePed)
        {
            int vehicleHandle = interactivePed.GetInteger(Client.DECOR_NPC_VEHICLE_HANDLE);

            if (vehicleHandle == 0) return;

            Vehicle vehicle = new Vehicle(vehicleHandle);

            Helpers.Animations.AnimationRadio();
            Wrappers.Helpers.ShowNotification("Dispatch", $"Running ~o~{vehicle.Mods.LicensePlate}", string.Empty);
            await Client.Delay(2000);

            bool stolen = interactivePed.GetBoolean(Client.DECOR_VEHICLE_STOLEN);

            string message = stolen ? "~r~STOLEN" : "~g~CLEAN";

            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Vehicle Report: {message}");
        }
    }
}
