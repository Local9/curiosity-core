using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class SpectateManager : Manager<SpectateManager>
    {
        bool isCurrentlySpectating = false;
        Player currentSpectate;
        Vector3 originalPosition = Vector3.Zero;

        public override void Begin()
        {
            EventSystem.Attach("spectate", new EventCallback(metadata =>
            {

                return null;
            }));
        }

        private async void Spectate(int playerId)
        {
            if (playerId == Cache.Player.Handle)
            {
                NotificationManger.GetModule().Warn("You cannot spectate yourself.");
                return;
            }

            await Utils.ScreenOptions.ScreenFadeOut(250);

            int playerPedId = GetPlayerPed(playerId);

            if (playerPedId == 0)
            {
                NotificationManger.GetModule().Warn("Unable to find player ped to spectate.");
                return;
            }

            if (currentSpectate is not null && currentSpectate?.Handle == playerPedId)
            {
                NetworkSetInSpectatorMode(false, playerPedId);
                SetMinimapInSpectatorMode(false, playerPedId);

                Cache.PlayerPed.IsVisible = true;
                Cache.PlayerPed.IsPositionFrozen = false;
                Cache.PlayerPed.Detach();

                Vector3 newNormal = Vector3.Zero;
                float positionZ = originalPosition.Z;

                if (GetGroundZAndNormalFor_3dCoord(originalPosition.X, originalPosition.Y, originalPosition.Z, ref positionZ, ref newNormal))
                {
                    originalPosition.Z = positionZ;
                }

                Cache.PlayerPed.Position = originalPosition;

                originalPosition = Vector3.Zero;
                currentSpectate = null;

                await Utils.ScreenOptions.ScreenFadeIn(250);
                await Cache.PlayerPed.FadeIn();

                Cache.PlayerPed.IsInvincible = false;

                return;
            }

            ClearPlayerWantedLevel(Cache.Player.Handle);
            originalPosition = Cache.PlayerPed.Position;

            await Cache.PlayerPed.FadeOut();

            Cache.PlayerPed.IsVisible = false;
            Cache.PlayerPed.IsPositionFrozen = true;
            Cache.PlayerPed.IsInvincible = true;

            NetworkSetInSpectatorMode(true, playerPedId);
            SetMinimapInSpectatorMode(true, playerPedId);

            await Utils.ScreenOptions.ScreenFadeIn(250);
        }
    }
}
