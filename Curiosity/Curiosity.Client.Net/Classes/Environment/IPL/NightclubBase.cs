using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment.IPL
{
    class NightclubBase
    {
        static Client client = Client.GetInstance();

        static Vector3 clubLocation = new Vector3(-1569.226f, -3017.124f, -74.40616f);
        static Vector3 clubEntrance = new Vector3(194.6124f, -3167.278f, 5.790269f);
        static Vector3 clubExit = new Vector3(-1569.665f, -3016.758f, -74.40615f);

        static WeatherTypes _lastWeather;

        public static void Init()
        {
            new BlipData(clubEntrance, (BlipSprite)614, Shared.Client.net.Enums.BlipCategory.Unknown, BlipColor.Blue, true);

            client.RegisterTickHandler(TeleportToClub);
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));

            client.RegisterEventHandler("curiosity:client:seasons:sync:weather", new Action<int, bool, int>(OnSeasonsWeatherSync));
        }

        private static void OnSeasonsWeatherSync(int weather, bool blackout, int temp)
        {
            _lastWeather = (WeatherTypes)weather;
        }

        static void OnPlayerSpawned()
        {
            if (GetInteriorFromEntity(Game.PlayerPed.Handle) == Nightclub.IplManager.NIGHTCLUB_INTERIOR_ID)
            {
                client.RegisterTickHandler(TeleportOutOfClub);
            }
        }

        static async Task TeleportToClub()
        {
            if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubEntrance, true) < 2.0)
            {
                if (Game.IsControlPressed(0, Control.Context))
                {
                    Client.TriggerEvent("curiosity:Player:World:FreezeTimer", true);
                    Screen.Fading.FadeOut(500);

                    DisableAllControlActions(0);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(100);
                    }

                    await Client.Delay(1000);

                    Game.PlayerPed.Position = clubExit;

                    await Client.Delay(1000);

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(100);
                    }

                    EnableAllControlActions(0);

                    client.RegisterTickHandler(TeleportOutOfClub);
                    client.DeregisterTickHandler(TeleportToClub);
                }

                NativeWrappers.DrawHelpText("Press ~INPUT_CONTEXT~ to ~b~enter the club");
            }
        }

        static async Task TeleportOutOfClub()
        {
            API.NetworkOverrideClockTime(0, 1, 0);
            API.SetOverrideWeather("FOGGY");
            Client.TriggerEvent("curiosity:Player:World:FreezeTimer", true);

            if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubExit, true) < 2.0)
            {
                if (Game.IsControlPressed(0, Control.Context))
                {
                    Client.TriggerEvent("curiosity:Player:World:FreezeTimer", false);

                    Screen.Fading.FadeOut(500);

                    API.SetOverrideWeather($"{_lastWeather}");

                    DisableAllControlActions(0);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(100);
                    }

                    await Client.Delay(1000);

                    Game.PlayerPed.Position = clubEntrance;

                    await Client.Delay(1000);

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(100);
                    }

                    EnableAllControlActions(0);

                    client.RegisterTickHandler(TeleportToClub);
                    client.DeregisterTickHandler(TeleportOutOfClub);
                }

                NativeWrappers.DrawHelpText("Press ~INPUT_PICKUP~ to ~b~leave the club");
            }
        }
    }
}
