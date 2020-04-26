using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class BackupMessages
    {
        static Client client = Client.GetInstance();

        static public bool IsAcceptingBackupCalls = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:BackupNotification", new Action<string, int, float, float, float>(BackupResponseMessage));
        }

        static async void BackupResponseMessage(string playerName, int code, float x, float y, float z)
        {
            if (!DutyManager.IsPoliceJobActive) return;
            if (!IsAcceptingBackupCalls) return;
            if (Game.Player.Name == playerName) return;

            bool showInformation = false;
            long gameTimer = GetGameTimer();

            while (!showInformation)
            {
                await Client.Delay(0);
                Screen.DisplayHelpTextThisFrame($"~r~Back Up Call~n~~w~Press ~INPUT_CONTEXT~ to see information...");

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    showInformation = true;
                }

                if ((GetGameTimer() - gameTimer) > 10000)
                {
                    break;
                }
            }

            if (showInformation)
            {

                string streetName = "Unkown Location";

                uint streetHash = 0;
                uint streetCrossing = 0;

                Vector3 pos = new Vector3(x, y, z);

                GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetHash, ref streetCrossing);

                streetName = World.GetStreetName(pos);
                string crossing = GetStreetNameFromHashKey(streetCrossing);
                string localisedZone = World.GetZoneLocalizedName(pos);

                if (!string.IsNullOrEmpty(localisedZone))
                {
                    streetName = $"{streetName}, {localisedZone}";
                }

                if (code == 4)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"CHAR_CALL911", 2, $"Dispatch", $"No further assistance required", $"Officer: ~b~{playerName}~n~~s~Code: ~b~{code}~n~~s~Loc: ~b~{streetName}", 2);
                    return;
                }

                string codeDetails = "Could not obtain";

                switch (code)
                {
                    case 2:
                        codeDetails = "~y~Urgent~s~ - Proceed immediately";
                        break;
                    case 3:
                        codeDetails = "~r~Emergency~s~ - Proceed immediately with lights and siren";
                        break;
                }

                Blip blip = World.CreateBlip(pos);
                blip.IsFlashing = true;
                blip.Sprite = (BlipSprite)458;
                blip.Color = BlipColor.Red;
                blip.Name = $"Back Up Request: {playerName}";

                SetNewWaypoint(pos.X, pos.Y);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"CHAR_CALL911", 2, $"Dispatch", $"Assistance Requested", $"Officer: ~b~{playerName}~n~~s~Code: ~b~{code}", 2);
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"CHAR_CALL911", 2, $"Dispatch", $"Details ~b~{playerName}", $"~s~Loc: ~b~{streetName}~n~{codeDetails}", 2);

                await Client.Delay(10000);
                blip.Delete();
            }
        }
    }
}
