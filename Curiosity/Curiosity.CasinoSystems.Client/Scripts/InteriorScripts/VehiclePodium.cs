using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts.InteriorScripts
{
    class VehiclePodium
    {
        static Plugin plugin;

        static Vehicle vehicle;
        static int platformEntityId;
        static float heading;

        static List<string> randomVehicle = new List<string>()
        {
            "thrax",
            "TURISMO2",
            "INFERNUS2",
            "jester3",
            "schlagen",
            "taipan",
            "gauntlet3",
            "stafford",
            "MAMBA",
            "swinger",
            "locust",
            "s80",
            "caracara2",
            "deveste",
            "neo",
            "stromberg",
            "krieger",
            "gauntlet4",
            "flashgt",
            "ARDENT",
            "drafter",
            "komoda",
            "everon",
            "emerus",
            "vstr",
            "comet4",
            "OPPRESSOR",
            "clique",
            "formula",
            "formula2"
        };

        public static void Init()
        {
            plugin = Plugin.GetInstance();

            plugin.RegisterTickHandler(OnTurnTable);
        }

        public static void Dispose()
        {
            RemoveVehicle();

            plugin.DeregisterTickHandler(OnTurnTable);

            plugin = null;
        }

        public static async Task OnTurnTable()
        {
            if (!API.DoesEntityExist(platformEntityId))
            {
                platformEntityId = API.GetClosestObjectOfType(1100f, 220f, -50f, 1f, (uint)API.GetHashKey("vw_prop_vw_casino_podium_01a"), false, false, false);

                CreateVehicle();
            }
            else
            {
                heading = (heading + (4f * API.Timestep()));

                if (heading >= 360)
                {
                    heading = heading - 360;
                }

                API.SetEntityHeading(platformEntityId, heading);
            }
        }

        public static async Task CreateVehicle()
        {
            RemoveVehicle();
            int veh;
            int loadChecks = 0;
            Model model = API.GetHashKey(randomVehicle[Plugin.random.Next(randomVehicle.Count)]); // server side setting
            model.Request(10000);

            while (!model.IsLoaded)
            {
                if (loadChecks > 10)
                    break;

                loadChecks++;
                await BaseScript.Delay(10);
            }

            if (model.IsLoaded)
            {
                veh = API.CreateVehicle((uint)model.Hash, 1100f, 220f, -50f, heading, false, true);
                vehicle = new Vehicle(veh);

                vehicle.IsPositionFrozen = true;
                vehicle.IsCollisionEnabled = false;
                vehicle.IsInvincible = true;
                API.N_0xab04325045427aae(vehicle.Handle, false);
                API.N_0xdbc631f109350b8c(vehicle.Handle, true);
                API.N_0x2311dd7159f00582(vehicle.Handle, true);

                API.AttachEntityToEntity(vehicle.Handle, platformEntityId, -1, 0f, 0f, GetVehicleZCoord(model.Hash), 0f, 0f, 0f, false, false, false, false, 2, true);
                model.MarkAsNoLongerNeeded();
            }
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
			return API.GetHashKey(name);
        }

		public static void RemoveVehicle()
        {
            if (vehicle != null)
            {
                if (vehicle.Exists())
                    vehicle.Delete();
            }
        }
    }
}
