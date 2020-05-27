using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.CasinoSystems.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts
{
    class Teleporters
    {
        static Plugin plugin;
        
        // am_mp_cassino.c
        // Line: 147986
        static Vector3 casinoEnterance = new Vector3(924.4668f, 46.7468f, 81.10635f);
        static float casinoEnteranceHeading = 341.6155f;
        // Line: 166421
        static Vector3 casinoExit = new Vector3(1089.974f, 206.0144f, -48.99975f);
        static float casinoExitHeading = 81.10635f;

        static float displayMarkerDistance = 50f;
        static float displayContextDistance = 1.5f;
        static Vector3 worldOffset = new Vector3(0f, 2f, 0f);

        // Distances
        static float distanceToEntrance = 10f;
        static float distanceToExit = 10f;

        static public bool isTeleporting = false;

        public static void Init()
        {
            plugin = Plugin.GetInstance();

            plugin.RegisterTickHandler(OnTeleportMarkersTick);
            plugin.RegisterTickHandler(OnTeleportDistanceTick);
        }

        private async static Task OnTeleportDistanceTick()
        {
            Vector3 playerPosition = Game.PlayerPed.Position;
            distanceToEntrance = playerPosition.Distance(casinoEnterance, true);
            distanceToExit = playerPosition.Distance(casinoExit, true);

            if (isTeleporting) return;

            if (distanceToEntrance < displayMarkerDistance)
            {
                DrawMarker(casinoEnterance);

                if (distanceToEntrance < displayContextDistance)
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to enter Casino.");

            }
            else if (distanceToExit < displayMarkerDistance)
            {
                DrawMarker(casinoExit);

                if (distanceToExit < displayContextDistance)
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to leave Casino.");
            }
        }

        private async static Task OnTeleportMarkersTick()
        {
            if (distanceToEntrance < displayContextDistance && Game.IsControlJustPressed(0, Control.Context))
            {
                TeleportPlayer(casinoExit, casinoEnteranceHeading);
            }
            else if (distanceToExit < displayContextDistance && Game.IsControlJustPressed(0, Control.Context))
            {
                TeleportPlayer(casinoEnterance, casinoExitHeading);
            }
        }

        private static void DrawMarker(Vector3 position)
        {
            float groundZ = position.Z;
            Vector3 groundNormal = Vector3.Zero;

            if (API.GetGroundZAndNormalFor_3dCoord(position.X, position.Y, position.Z, ref groundZ, ref groundNormal))
            {
                position.Z = groundZ;
            }

            World.DrawMarker(MarkerType.VerticalCylinder, position, Vector3.Zero, Vector3.Zero, Vector3.One, System.Drawing.Color.FromArgb(173, 216, 230));
        }

        private async static void TeleportPlayer(Vector3 position, float heading = 0f)
        {
            isTeleporting = true;

            Screen.Fading.FadeOut(1000);
            while(!Screen.Fading.IsFadedOut)
            {
                await BaseScript.Delay(10);
            }

            Game.PlayerPed.Heading = heading;

            Vector3 ground = position;
            ground.Z = position.Z - 1.2f;
            Game.PlayerPed.Position = ground;

            await BaseScript.Delay(500);

            Screen.Fading.FadeIn(1000);
            await BaseScript.Delay(500);

            Game.PlayerPed.Task.GoTo(Game.PlayerPed.GetOffsetPosition(worldOffset));

            while (!Screen.Fading.IsFadedIn)
            {
                await BaseScript.Delay(10);
            }

            isTeleporting = false;
        }
    }
}
