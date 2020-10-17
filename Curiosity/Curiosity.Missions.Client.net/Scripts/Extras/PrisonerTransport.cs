using CitizenFX.Core;
using Curiosity.Global.Shared.Utils;

namespace Curiosity.Missions.Client.Scripts.Extras
{
    class PrisonerTransport
    {
        // STATE
        static bool IsServiceActive = false;
        static public bool HasPedBeenPickedUp = false;

        // ENTITIES
        static Ped PedToRecover;

        // Coroner Stuff
        static Ped CoronerDriver;
        static Vehicle CoronerVehicle;

        static PedHash CoronerDriverHash = PedHash.Cop01SMY;
        static VehicleHash CoronerVehicleHash = VehicleHash.PoliceT;

        static public void RequestService()
        {
            //if (!Police.ArrestPed.IsPedCuffed)
            //{
            //    Wrappers.Helpers.ShowNotification("Dispatch", "Must be in cuffs", string.Empty);
            //    return;
            //}

            //if (Police.TrafficStop.StoppedDriver.CurrentVehicle != PluginInstance.CurrentVehicle)
            //{
            //    Wrappers.Helpers.ShowNotification("Dispatch", "Must be detained", string.Empty);
            //    return;
            //}

            if (IsServiceActive)
            {
                Wrappers.Helpers.ShowNotification("Dispatch", "Service Unavailable", string.Empty);
                return;
            }

            int spawnDistance = Utility.RANDOM.Next(300, 800);
            RaycastResult raycastResult = World.RaycastCapsule(Game.PlayerPed.Position, Game.PlayerPed.Position, 5.0f, IntersectOptions.Peds1, Game.Player.Character);


        }
    }
}
