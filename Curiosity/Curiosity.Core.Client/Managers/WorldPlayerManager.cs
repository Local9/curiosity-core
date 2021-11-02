using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Models;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class WorldPlayerManager : Manager<WorldPlayerManager>
    {
        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async Task OnPlayerInteractionTick()
        {
            Vector3 playerPosition = Game.PlayerPed.Position;
            Player myPlayer = Game.Player;

            foreach (Player player in Instance.PlayerList)
            {
                if (player == myPlayer) continue; // ignore self
                if (!NetworkIsPlayerActive(player.Handle)) continue;

                if (!player.Character.Exists()) continue; // if they don't exist, ignore

                Ped playerPed = player.Character;

                CanRevivePlayer(playerPosition, playerPed, player.ServerId, myPlayer);
            }
        }

        private async void CanRevivePlayer(Vector3 playerPosition, Ped playerPed, int serverId, Player myPlayer)
        {
            if (myPlayer.Character.IsInVehicle()) return;
            if (myPlayer.Character.IsDead) return;

            if (!playerPed.Exists()) return;
            if (!playerPed.IsDead) return;
            if (!playerPed.IsInRangeOf(playerPosition, 2f)) return;

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to attempt revive.");

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                ExportMessage exportMessage = await EventSystem.Request<ExportMessage>("character:revive:other", serverId);

                if (!exportMessage.Success)
                {
                    NotificationManager.GetModule().Error(exportMessage.Error);
                }
            }
        }
    }
}
