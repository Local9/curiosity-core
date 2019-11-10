using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment.IPL
{
    class NightclubBase
    {
        static Client client = Client.GetInstance();

        static Vector3 clubLocation = new Vector3(-1569.226f, -3017.124f, -74.40616f);
        static Vector3 clubEntrance = new Vector3(194.6124f, -3167.278f, 5.790269f);
        static Vector3 clubExit = new Vector3(-1569.665f, -3016.758f, -74.40615f);

        public static void Init()
        {
            new BlipData(clubEntrance, (BlipSprite)614, Shared.Client.net.Enums.BlipCategory.Unknown, BlipColor.Blue, true).Create();
            client.RegisterTickHandler(TeleportToClub);

            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
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
                    Screen.Fading.FadeOut(500);

                    DisableAllControlActions(0);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(0);
                    }

                    Game.PlayerPed.Position = clubExit;

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(0);
                    }

                    EnableAllControlActions(0);

                    client.RegisterTickHandler(TeleportOutOfClub);
                    client.DeregisterTickHandler(TeleportToClub);
                }

                NativeWrappers.DrawHelpText("Press ~INPUT_CONTEXT~ to ~b~enter the club");
            }

            await Task.FromResult(0);
        }

        static async Task TeleportOutOfClub()
        {
            if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, clubExit, true) < 2.0)
            {
                if (Game.IsControlPressed(0, Control.Context))
                {
                    Screen.Fading.FadeOut(500);

                    DisableAllControlActions(0);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(0);
                    }

                    Game.PlayerPed.Position = clubEntrance;

                    Screen.Fading.FadeIn(500);
                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(0);
                    }

                    EnableAllControlActions(0);

                    client.RegisterTickHandler(TeleportToClub);
                    client.DeregisterTickHandler(TeleportOutOfClub);
                }

                NativeWrappers.DrawHelpText("Press ~INPUT_PICKUP~ to ~b~leave the club");
            }

            await Task.FromResult(0);
        }
    }
}
