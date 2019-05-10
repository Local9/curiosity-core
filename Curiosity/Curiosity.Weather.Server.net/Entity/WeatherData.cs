namespace Curiosity.Server.Net.Entity
{
    public class WeatherData
    {
        public string CurrentWeather;
        public bool Wind;
        public float WindHeading;

        public override string ToString()
        {
            return $"CurrentWeather: {CurrentWeather}, Wind: {Wind}, Heading: {WindHeading}";
        }
    }
}
