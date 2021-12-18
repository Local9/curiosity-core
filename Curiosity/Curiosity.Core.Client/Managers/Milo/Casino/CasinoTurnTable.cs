using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.Casino
{
    class CasinoTurnTable
    {
        static Vehicle vehicle;
        static string vehicleModel = string.Empty;
        static int platformHandle;
        static float heading;

        static PluginManager PluginManager => PluginManager.Instance;

        internal static async void Init()
        {
            PluginManager.AttachTickHandler(OnTurnTableTask);
        }

        internal static void Dispose()
        {
            RemoveVehicle();
            PluginManager.DetachTickHandler(OnTurnTableTask);
        }

        private async static Task OnTurnTableTask()
        {
            if (!DoesEntityExist(platformHandle))
            {
                platformHandle = GetClosestObjectOfType(1100f, 220f, -50f, 1f, (uint)GetHashKey("vw_prop_vw_casino_podium_01a"), false, false, false);

                CreateVehicleForDisplay();
            }
            else
            {
                heading = (heading + (4f * Timestep()));

                if (heading >= 360)
                {
                    heading = heading - 360;
                }

                SetEntityHeading(platformHandle, heading);
            }
        }

        static async Task CreateVehicleForDisplay()
        {
            RemoveVehicle();
            await BaseScript.Delay(100);
            // update if the server changed it

            if (string.IsNullOrEmpty(vehicleModel))
                vehicleModel = await EventSystem.GetModule().Request<dynamic>("casino:vehicle");

            Logger.Debug($"Vehicle Model: {vehicleModel}");

            int veh;
            int loadChecks = 0;
            Model model = new Model(vehicleModel);
            await model.Request(10000);

            while (!model.IsLoaded)
            {
                if (loadChecks > 10)
                    break;

                loadChecks++;
                await BaseScript.Delay(10);
            }

            if (model.IsLoaded && vehicle is null)
            {
                veh = CreateVehicle((uint)model.Hash, 1100f, 220f, -50f, heading, false, true);
                vehicle = new Vehicle(veh);

                vehicle.IsPositionFrozen = true;
                vehicle.IsCollisionEnabled = false;
                vehicle.IsInvincible = true;
                N_0xab04325045427aae(vehicle.Handle, false);
                N_0xdbc631f109350b8c(vehicle.Handle, true);
                N_0x2311dd7159f00582(vehicle.Handle, true);

                AttachEntityToEntity(vehicle.Handle, platformHandle, -1, 0f, 0f, GetVehicleZCoord(model.Hash), 0f, 0f, 0f, false, false, false, false, 2, true);
            }

            model.MarkAsNoLongerNeeded();
        }

        static float GetVehicleZCoord(int modelHash)
        {
            if (modelHash == GetVehicleHash("thrax"))
                return 0.55f;
            if (modelHash == GetVehicleHash("TURISMO2"))
                return 0.88f;
            if (modelHash == GetVehicleHash("INFERNUS2"))
                return 0.78f;
            if (modelHash == GetVehicleHash("jester3"))
                return 0.54f;
            if (modelHash == GetVehicleHash("schlagen"))
                return 0.54f;
            if (modelHash == GetVehicleHash("taipan"))
                return 0.64f;
            if (modelHash == GetVehicleHash("gauntlet3"))
                return 0.605f;
            if (modelHash == GetVehicleHash("stafford"))
                return 0.758f;
            if (modelHash == GetVehicleHash("MAMBA"))
                return 0.54f;
            if (modelHash == GetVehicleHash("swinger"))
                return 0.62f;
            if (modelHash == GetVehicleHash("locust"))
                return 0.75f;
            if (modelHash == GetVehicleHash("s80"))
                return 0.46f;
            if (modelHash == GetVehicleHash("caracara2"))
                return 0.98f;
            if (modelHash == GetVehicleHash("deveste"))
                return 0.58f;
            if (modelHash == GetVehicleHash("neo"))
                return 0.61f;
            if (modelHash == GetVehicleHash("stromberg"))
                return 0.51f;
            if (modelHash == GetVehicleHash("krieger"))
                return 0.82f;
            if (modelHash == GetVehicleHash("gauntlet4"))
                return 0.65f;
            if (modelHash == GetVehicleHash("flashgt"))
                return 1.06f;
            if (modelHash == GetVehicleHash("ARDENT"))
                return 0.61f;
            if (modelHash == GetVehicleHash("drafter"))
                return 0.76f;
            if (modelHash == GetVehicleHash("komoda"))
                return 0.61f;
            if (modelHash == GetVehicleHash("everon"))
                return 1.28f;
            if (modelHash == GetVehicleHash("emerus"))
                return 0.55f;
            if (modelHash == GetVehicleHash("vstr"))
                return 0.67f;
            if (modelHash == GetVehicleHash("comet4"))
                return 0.69f;
            if (modelHash == GetVehicleHash("OPPRESSOR"))
                return 0.79f;
            if (modelHash == GetVehicleHash("clique"))
                return 0.59f;
            if (modelHash == GetVehicleHash("formula"))
                return 0.52f;
            if (modelHash == GetVehicleHash("formula2"))
                return 0.52f;

            return 0.5f;
        }

        static int GetVehicleHash(string name)
        {
            return GetHashKey(name);
        }

        public static void RemoveVehicle()
        {
            if (vehicle != null)
            {
                if (vehicle.Exists())
                    vehicle.Delete();

                vehicle = null;
            }
        }
    }
}
