namespace Curiosity.Core.Client.Managers
{
    public class WorldPlayerReviveManager : Manager<WorldPlayerReviveManager>
    {
        bool processing = false;

        public override void Begin()
        {

        }

        //[TickHandler(SessionWait = true)]
        //private async Task OnPlayerInteractionTick()
        //{
        //    Vector3 playerPosition = Game.PlayerPed.Position;
        //    Player myPlayer = Game.Player;

        //    foreach (Player player in Instance.PlayerList)
        //    {
        //        if (player == myPlayer) continue; // ignore self
        //        if (!NetworkIsPlayerActive(player.Handle)) continue;

        //        if (!player.Character.Exists()) continue; // if they don't exist, ignore

        //        Ped playerPed = player.Character;

        //        CanRevivePlayer(playerPosition, playerPed, player.ServerId, myPlayer);
        //    }
        //}

        //private async void CanRevivePlayer(Vector3 playerPosition, Ped playerPed, int serverId, Player myPlayer)
        //{
        //    if (myPlayer.Character.IsInVehicle()) return;
        //    if (myPlayer.Character.IsDead) return;

        //    if (!playerPed.Exists()) return;
        //    if (!playerPed.IsDead) return;
        //    if (!playerPed.IsInRangeOf(playerPosition, 2f)) return;

        //    if (!processing)
        //        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to attempt revive.");

        //    if (Game.IsControlJustPressed(0, Control.Context))
        //    {
        //        Screen.DisplayHelpTextThisFrame($"Attempting to revive player.");
        //        if (processing) return;
        //        processing = true;

        //        ExportMessage exportMessage = await EventSystem.Request<ExportMessage>("character:revive:other", serverId);

        //        if (!exportMessage.success)
        //        {
        //            NotificationManager.GetModule().Error(exportMessage.error);
        //            await BaseScript.Delay(1000);
        //        }

        //        if (exportMessage.success)
        //        {
        //            Screen.DisplayHelpTextThisFrame($"Player has been revived.");
        //            await BaseScript.Delay(1000);
        //        }

        //        processing = false;
        //    }
        //}
    }
}
