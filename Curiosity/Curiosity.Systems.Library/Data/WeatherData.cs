using Curiosity.Systems.Library.Attributes;
using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Data
{
    public class WeatherData
    {
        public readonly static Dictionary<WeatherMonth, WeatherSeason> SeasonalMonth = new Dictionary<WeatherMonth, WeatherSeason>()
        {
            { WeatherMonth.JANUARY, WeatherSeason.SPRING },
            { WeatherMonth.FEBRUARY, WeatherSeason.SPRING },
            { WeatherMonth.MARCH, WeatherSeason.SPRING },
            { WeatherMonth.APRIL, WeatherSeason.SUMMER },
            { WeatherMonth.MAY, WeatherSeason.SUMMER },
            { WeatherMonth.JUNE, WeatherSeason.SUMMER },
            { WeatherMonth.JULY, WeatherSeason.SUMMER },
            { WeatherMonth.AUGUST, WeatherSeason.SUMMER },
            { WeatherMonth.SEPTEMBER, WeatherSeason.AUTUMN },
            { WeatherMonth.OCTOBER, WeatherSeason.AUTUMN },
            { WeatherMonth.NOVEMBER, WeatherSeason.AUTUMN },
            { WeatherMonth.DECEMBER, WeatherSeason.WINTER },
        };

        public readonly static List<int> SnowDays = new List<int>() { 6, 9, 12, 15, 18, 21, 22, 23, 24, 25, 26, 27 };
        public readonly static List<int> HalloweenDays = new List<int>() { 30, 31 };

        public readonly static Dictionary<WeatherSeason, List<WeatherType>> SeasonalWeather = new Dictionary<WeatherSeason, List<WeatherType>>()
        {
            { WeatherSeason.SPRING, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.OVERCAST, WeatherType.SMOG } },
            { WeatherSeason.SUMMER, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.SMOG } },
            { WeatherSeason.AUTUMN, new List<WeatherType>() { WeatherType.OVERCAST, WeatherType.SMOG, WeatherType.CLEAR, WeatherType.FOGGY } },
            { WeatherSeason.WINTER, new List<WeatherType>() { WeatherType.SMOG, WeatherType.SNOWING } },
        };

        public readonly static Dictionary<Region, List<WeatherType>> RegionWeather = new Dictionary<Region, List<WeatherType>>()
        {
            { Region.NorthYankton, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.RAINING, WeatherType.OVERCAST, WeatherType.CHRISTMAS, WeatherType.SNOWING, WeatherType.SNOWLIGHT } },
            { Region.CayoPericoIsland, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.RAINING, WeatherType.OVERCAST, WeatherType.CLEARING } },
        };

        public static WeatherSeason GetCurrentSeason()
        {
            return SeasonalMonth[(WeatherMonth)DateTime.UtcNow.Month];
        }

        public static bool IsSnowDay()
        {
            return SnowDays.Contains(DateTime.UtcNow.Day) && DateTime.UtcNow.Month == 12;
        }

        public static bool IsHalloween()
        {
            return HalloweenDays.Contains(DateTime.UtcNow.Day) && DateTime.UtcNow.Month == 10;
        }
    }

    public enum WeatherSeason
    {
        [StringValue("Winter")]
        WINTER,
        [StringValue("Spring")]
        SPRING,
        [StringValue("Summer")]
        SUMMER,
        [StringValue("Autumn")]
        AUTUMN
    }

    public enum WeatherMonth
    {
        JANUARY = 1,
        FEBRUARY,
        MARCH,
        APRIL,
        MAY,
        JUNE,
        JULY,
        AUGUST,
        SEPTEMBER,
        OCTOBER,
        NOVEMBER,
        DECEMBER
    }

    public enum WeatherType
    {
        UNKNOWN = -1,
        EXTRASUNNY,
        CLEAR,
        CLOUDS,
        SMOG,
        FOGGY,
        OVERCAST,
        RAINING,
        THUNDERSTORM,
        CLEARING,
        NEUTRAL,
        SNOWING,
        BLIZZARD,
        SNOWLIGHT,
        CHRISTMAS,
        HALLOWEEN
    }
}
