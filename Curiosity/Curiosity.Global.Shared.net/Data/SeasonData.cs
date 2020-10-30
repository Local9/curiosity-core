using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Global.Shared.Data
{
    public enum WeatherTypes
    {
        EXTRASUNNY,
        CLEAR,
        NEUTRAL,
        SMOG,
        FOGGY,
        OVERCAST,
        CLOUDS,
        CLEARING,
        RAIN,
        THUNDER,
        SNOW,
        BLIZZARD,
        SNOWLIGHT,
        XMAS,
        HALLOWEEN,
        XMAS_STORM
    }

    public enum Seasons
    {
        SPRING,
        SUMMER,
        AUTUMN,
        WINTER
    }

    static public class SeasonData
    {
        static public List<Seasons> SeasonList()
        {
            return Enum.GetValues(typeof(Seasons)).Cast<Seasons>().ToList();
        }
        static public List<WeatherTypes> WeatherList()
        {
            return Enum.GetValues(typeof(WeatherTypes)).Cast<WeatherTypes>().ToList();
        }
        static public List<WeatherTypes> WeatherSpringList()
        {
            return new List<WeatherTypes>()
            {
                WeatherTypes.CLEAR,
                WeatherTypes.CLEARING,
                WeatherTypes.OVERCAST,
                WeatherTypes.CLOUDS,
                WeatherTypes.EXTRASUNNY,
                WeatherTypes.RAIN,
                WeatherTypes.FOGGY,
            };
        }
        static public List<WeatherTypes> WeatherSummerList()
        {
            return new List<WeatherTypes>()
            {
                WeatherTypes.CLEAR,
                WeatherTypes.CLEARING,
                WeatherTypes.EXTRASUNNY,
                WeatherTypes.SMOG,
            };
        }
        static public List<WeatherTypes> WeatherAutumnList()
        {
            return new List<WeatherTypes>()
            {
                WeatherTypes.CLEAR,
                WeatherTypes.CLEARING,
                WeatherTypes.OVERCAST,
                WeatherTypes.CLOUDS,
                WeatherTypes.EXTRASUNNY,
                WeatherTypes.RAIN,
                WeatherTypes.FOGGY,
                WeatherTypes.THUNDER
            };
        }
    }
}
