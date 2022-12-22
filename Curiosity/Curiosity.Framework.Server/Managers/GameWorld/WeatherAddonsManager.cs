namespace Curiosity.Framework.Server.Managers.GameWorld
{
    public class WeatherAddonsManager : Manager<WeatherAddonsManager>
    {
        public override void Begin()
        {
            SetAddons();
        }

        void SetAddons()
        {
            bool isWinter = false;

            string stateAlamo = GetResourceState("nve_iced_alamo");
            string stateXmas = GetResourceState("nve_xmas");

            if (stateAlamo == "started" && !isWinter)
                StopResource("nve_iced_alamo");

            if (stateXmas == "started" && !isWinter)
                StopResource("nve_xmas");

            if (stateAlamo == "stopped" && isWinter)
                StartResource("nve_iced_alamo");

            if (stateXmas == "stopped" && isWinter)
                StartResource("nve_xmas");
        }
    }
}
