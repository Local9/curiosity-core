using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Shared.Client.net.Helper;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment
{
    static class AfkKick
    {
        static Vector3 lastPlayerLocation = new Vector3();
        static int lastMovement = 0;
        static int lastKeyPress = 0;
        static float afkWarning = 5f; // How many minutes before to warn player

        // Also, if dev don't kick
        static public void Init()
        {
            lastMovement = lastKeyPress = API.GetGameTimer();
            // Client.GetInstance().RegisterTickHandler(OnTick);
            MinuteLoop();
        }

        static public Task OnPushToTalkTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.PushToTalk) || ControlHelper.IsControlJustPressed(Control.MpTextChatAll))
            {
                lastKeyPress = Function.Call<int>(Hash.GET_GAME_TIMER);
            }
            return Task.FromResult(0);
        }

        static public async Task MinuteLoop()
        {
            while (true)
            {
                await BaseScript.Delay(60000);

                if (Client.minutesAfkKick == 0)
                {
                    break; // disable AFK timer
                }

                if (PlayerInformation.IsStaff())
                    return;

                if (lastPlayerLocation != (lastPlayerLocation = Game.PlayerPed.Position))
                {
                    lastMovement = API.GetGameTimer();
                }

                int currentTime = API.GetGameTimer();

                if ((currentTime - lastMovement) > 60000 * Client.minutesAfkKick)
                {
                    BaseScript.TriggerServerEvent("curiosity:Server:Player:AfkKick");
                }
                else if ((currentTime - lastMovement) > 60000 * (Client.minutesAfkKick - afkWarning))
                {
                    float timeRemaining = (currentTime - lastMovement) / 60000f;
                    BaseScript.TriggerEvent("curiosity:Client:Chat:Message", "", "#FF0000", $@"You will be kicked for being AFK in {(Client.minutesAfkKick - timeRemaining):0} minute{(Client.minutesAfkKick - timeRemaining > 1 ? "s" : "")}.");
                }

                //if (((currentTime - lastMovement) > 60000 * afkLimit || (currentTime - lastKeyPress) > 60000 * afkLimit))
                //{
                //    BaseScript.TriggerServerEvent("AfkKick.RequestDrop");
                //}
                //else if ((currentTime - lastMovement) > 60000 * (afkLimit - afkWarning) || (currentTime - lastKeyPress) > 60000 * (afkLimit - afkWarning))
                //{
                //    float timeRemaining = Math.Max((currentTime - lastMovement), (currentTime - lastKeyPress)) / 60000f;
                //    BaseScript.TriggerEvent("curiosity:Client:Chat:Message", "", "#FF0000", $@"You will be kicked for being AFK in {(afkLimit - timeRemaining):0} minute{(afkLimit - timeRemaining > 1 ? "s" : "")}.");
                //}
            }
        }
    }
}
