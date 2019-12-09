using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Scripts.Interactions.VehicleInteractions;
namespace Curiosity.Missions.Client.net.Scripts.Interactions.DispatchInteractions
{
    class DispatchCenter
    {
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
                Client.TriggerEvent("curiosity:interaction:idRan", interactivePed.NetworkId);
            }

            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Name: ~y~{interactivePed.Name}~w~\nGender: ~b~{interactivePed.Ped.Gender}~w~\nDOB: ~b~{interactivePed.DateOfBirth}");
            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Citations: {interactivePed.NumberOfCitations}\n~w~Flags: {interactivePed.Offence}");
        }
    }
}
