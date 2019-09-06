using CitizenFX.Core;
using System;
using System.Linq;

namespace Curiosity.Client.net.Classes.Environment
{
    class InstancingChecker
    {
        static PlayerList playerList;
        static bool lastCheckFailed = false;
        static Client client = Client.GetInstance();

        public static void Init()
        {
            playerList = Client.players;

            client.RegisterEventHandler("curiosity:Client:Settings:PlayerCount", new Action<int>(PlayerCountUpdate));
            // Server sends its playercount to all clients once a minute to trigger InstancingChecker.PlayerCount
            // Compare player numbers, and if player count locally is lower TWICE IN A ROW
            // yell at player and trigger the servertrigger InstancingChecker.RequestDrop after N seconds

            // If this does not work as intended when we go live just disable it completely
        }

        static public async void PlayerCountUpdate(int remoteCount)
        {
            int localCount = playerList.Count();
            if (localCount < remoteCount)
            {
                if (lastCheckFailed)
                {
                    Classes.Environment.UI.Notifications.Curiosity(1, "Curiosity", "Instanced", "You're instanced and will be dropped in 60s. Please reconnect now or after.", 8);
                    await BaseScript.Delay(60000);
                    BaseScript.TriggerServerEvent("curiosity:Server:Player:Instance");
                }
                else
                {
                    lastCheckFailed = true;
                }
            }
            else
            {
                lastCheckFailed = false;
            }
        }
    }
}
