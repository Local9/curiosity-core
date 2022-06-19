namespace Curiosity.Core.Client.Utils
{
    public class Vehicles : BaseScript
    {
        [Flags]
        public enum Filter
        {
            None = 0,
            WithDriver = 1,
            PlayerVehicle = 2
        }

        [Flags]
        public enum StateFlag
        {
            HazardLight = 1,
            TiresIntact = 2,
            EngineIntact = 4,
            Cruising = 8,
            EMP = 16
        }

        [Flags]
        public enum FeatureFlag
        {
            BackToTheFuture = 1,
            TurboBoost = 2,
            TurboBrake = 4,
            AntiGravity = 8,
            RemoteControl = 16,
            JesusMode = 32,
            MosesMode = 64,
            Stabilizer = 128
        }

        public enum Light
        {
            Headlight,
            LeftIndicator,
            RightIndicator,
            HazardLight
        }

        public delegate void StateChangedDelegate(int vehicle, StateFlag states);
        public static event StateChangedDelegate StateChanged;

        public delegate void FeatureChangedDelegate(int vehicle, FeatureFlag features);
        public static event FeatureChangedDelegate FeatureChanged;

        public const Filter DefaultFilters = Filter.PlayerVehicle;

        public Vehicles()
        {

        }

        public static List<int> Get(Filter exclude = DefaultFilters, float rangeSquared = 3600f)
        {
            var vehicles = new List<int>();
            int vehicle = 0;
            int handle = API.FindFirstVehicle(ref vehicle);
            var player = API.GetPlayerPed(-1);
            var playerVehicle = API.GetVehiclePedIsIn(player, false);
            var coords = API.GetEntityCoords(player, true);

            if (handle == -1)
                return vehicles;

            bool HasFilter(Filter filter)
            {
                return (exclude & filter) == filter;
            }

            do
            {
                var pos = API.GetEntityCoords(vehicle, false);

                if (HasFilter(Filter.PlayerVehicle) && vehicle == playerVehicle)
                    continue;

                if (HasFilter(Filter.WithDriver) && !API.IsVehicleSeatFree(vehicle, -1))
                    continue;

                if (rangeSquared > 0f && coords.DistanceToSquared(pos) > rangeSquared)
                    continue;

                vehicles.Add(vehicle);

            } while (API.FindNextVehicle(handle, ref vehicle));

            API.EndFindVehicle(handle);
            return vehicles;
        }

        public static bool GetFreeSeat(int vehicle, out int seat, bool forcePassenger = false)
        {
            var model = (uint)API.GetEntityModel(vehicle);
            int seats = API.GetVehicleModelNumberOfSeats(model) - 1;
            int minSeat = -1;

            if (forcePassenger)
            {
                minSeat = (seats > 1) ? 1 : 0;
            }

            for (seat = minSeat; seat < seats; ++seat)
            {
                if (API.IsVehicleSeatFree(vehicle, seat))
                    return true;
            }

            if (forcePassenger && API.IsVehicleSeatFree(vehicle, 0))
            {
                seat = 0;
                return true;
            }

            return false;
        }

        public static Queue<int> GetFreeSeats(int vehicle)
        {
            var model = (uint)API.GetEntityModel(vehicle);
            int seats = API.GetVehicleModelNumberOfSeats(model) - 1;
            var freeSeats = new Queue<int>();

            for (int seat = -1; seat < seats; ++seat)
            {
                if (API.IsVehicleSeatFree(vehicle, seat))
                    freeSeats.Enqueue(seat);
            }

            return freeSeats;
        }

        public static bool GetPedSeat(int vehicle, int ped, out int seat)
        {
            var model = (uint)API.GetEntityModel(vehicle);
            var seats = API.GetVehicleModelNumberOfSeats(model) - 1;

            for (seat = -1; seat < seats; ++seat)
            {
                var seatPed = API.GetPedInVehicleSeat(vehicle, seat);
                if (seatPed == ped)
                    return true;
            }

            return false;
        }

        public static List<int> GetPlayers(int vehicle)
        {
            var players = new List<int>();
            var model = (uint)API.GetEntityModel(vehicle);
            var seats = API.GetVehicleModelNumberOfSeats(model) - 1;

            for (int seat = -1; seat < seats; ++seat)
            {
                var ped = API.GetPedInVehicleSeat(vehicle, seat);
                if (API.IsPedAPlayer(ped))
                    players.Add(API.NetworkGetPlayerIndexFromPed(ped));
            }

            return players;
        }

        private static async Task NetCompress(int netVehicle)
        {
            var vehicle = API.NetToVeh(netVehicle);
            if (!API.DoesEntityExist(vehicle))
                return;

            var model = (uint)API.GetEntityModel(vehicle);
            var min = Vector3.Zero;
            var max = Vector3.Zero;
            API.GetModelDimensions(model, ref min, ref max);

            var front = API.GetOffsetFromEntityInWorldCoords(vehicle, 0f, max.Y, 0f);
            var rear = API.GetOffsetFromEntityInWorldCoords(vehicle, 0f, min.Y, 0f);
            var left = API.GetOffsetFromEntityInWorldCoords(vehicle, min.X, 0f, 0f);
            var right = API.GetOffsetFromEntityInWorldCoords(vehicle, max.X, 0f, 0f);
            var top = API.GetOffsetFromEntityInWorldCoords(vehicle, 0f, 0f, max.Z);

            var sides = new Vector3[] { front, rear, left, right, top };

            for (int i = 0; i < 5; ++i)
            {
                foreach (var side in sides)
                {
                    API.SetVehicleDamage(vehicle, side.X, side.Y, side.Z, 100000f, i, false);
                }

                await Delay(100);
            }
        }

        public static bool AreTiresIntact(int vehicle)
        {
            var wheels = API.GetVehicleNumberOfWheels(vehicle);
            for (int wheel = 0; wheel < wheels; ++wheel)
            {
                if (API.IsVehicleTyreBurst(vehicle, wheel, false))
                    return false;
            }

            return true;
        }

        private static bool IsHazardLightApplicable(uint model)
        {
            return API.IsThisModelACar(model) || API.IsThisModelABike(model) || API.IsThisModelAnAmphibiousCar(model) || API.IsThisModelAQuadbike(model);
        }
    }
}
