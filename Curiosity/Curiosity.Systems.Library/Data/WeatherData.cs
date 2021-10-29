﻿using Curiosity.Systems.Library.Attributes;
using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Data
{
    public class WeatherData
    {
        public readonly static Dictionary<WeatherMonth, WeatherSeason> SeasonalMonth = new Dictionary<WeatherMonth, WeatherSeason>()
        {
            { WeatherMonth.JANUARY, WeatherSeason.WINTER },
            { WeatherMonth.FEBRUARY, WeatherSeason.WINTER },
            { WeatherMonth.MARCH, WeatherSeason.SPRING },
            { WeatherMonth.APRIL, WeatherSeason.SPRING },
            { WeatherMonth.MAY, WeatherSeason.SPRING },
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
            { WeatherSeason.SPRING, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.CLOUDS, WeatherType.OVERCAST, WeatherType.RAIN } },
            { WeatherSeason.SUMMER, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.CLOUDS } },
            { WeatherSeason.AUTUMN, new List<WeatherType>() { WeatherType.CLOUDS, WeatherType.OVERCAST, WeatherType.SMOG, WeatherType.CLEAR, WeatherType.FOGGY } },
            { WeatherSeason.WINTER, new List<WeatherType>() { WeatherType.SMOG, WeatherType.FOGGY, WeatherType.SNOWLIGHT, WeatherType.BLIZZARD } },
        };

        public readonly static Dictionary<Region, List<WeatherType>> RegionWeather = new Dictionary<Region, List<WeatherType>>()
        {
            { Region.NorthYankton, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.CLOUDS, WeatherType.OVERCAST, WeatherType.SNOW, WeatherType.SNOWLIGHT, WeatherType.BLIZZARD, WeatherType.XMAS } },
            { Region.CayoPericoIsland, new List<WeatherType>() { WeatherType.CLEAR, WeatherType.EXTRASUNNY, WeatherType.CLOUDS, WeatherType.OVERCAST, WeatherType.RAIN } },
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
        [StringValue("Unknown")]
        UNKNOWN,
        [StringValue("Sunny")]
        EXTRASUNNY,
        [StringValue("Clear")]
        CLEAR,
        [StringValue("Clear")]
        NEUTRAL,
        [StringValue("Smog")]
        SMOG,
        [StringValue("Foggy")]
        FOGGY,
        [StringValue("Overcast")]
        OVERCAST,
        [StringValue("Cloudy")]
        CLOUDS,
        [StringValue("Clear")]
        CLEARING,
        [StringValue("Rain")]
        RAIN,
        [StringValue("Storm")]
        THUNDER,
        [StringValue("Blizzard")]
        BLIZZARD,
        [StringValue("Light Snow")]
        SNOWLIGHT,
        [StringValue("X-Mas Snow")]
        XMAS,
        [StringValue("Snow")]
        SNOW,
        [StringValue("Halloween")]
        HALLOWEEN
    }
}
