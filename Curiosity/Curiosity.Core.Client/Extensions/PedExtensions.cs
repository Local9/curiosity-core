namespace Curiosity.Core.Client.Extensions
{
    public static class PedExtensions
    {
        public static async Task<string> GetHeadshot(this Ped ped)
        {
            int headshot = API.RegisterPedheadshot(ped.Handle);

            while (!API.IsPedheadshotReady(headshot))
            {
                await BaseScript.Delay(0);
            }

            return API.GetPedheadshotTxdString(headshot);
        }

        public async static Task FadeOut(this Ped ped, bool slow = true)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = true)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = true)
        {
            if (fadeIn)
            {
                Function.Call((Hash)0x1F4ED342ACEFE62D, ped.Handle, fadeIn, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            while (API.NetworkIsEntityFading(ped.Handle))
            {
                await BaseScript.Delay(10);
            }
        }

        public static Vehicle[] GetNearbyVehicles(this Ped ped, float radius)
        {
            Vehicle[] vehicles = World.GetAllVehicles();
            var result = new List<Vehicle>();
            Vehicle ignore = ped.CurrentVehicle;
            int ignoreHandle = Vehicle.Exists(ignore) ? ignore.Handle : 0;
            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.Handle == ignoreHandle)
                {
                    continue;
                }
                if (Game.PlayerPed.IsInRangeOf(vehicle.Position, radius))
                    result.Add(vehicle);
            }
            return result.ToArray();
        }
    }
}
