using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class BackupMessages
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:BackupNotification", new Action<string, int, float, float, float>(BackupResponseMessage));
        }

        static void BackupResponseMessage(string playerName, int code, float x, float y, float z)
        {
            if (!DutyManager.IsPoliceJobActive) return;

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

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"CHAR_CALL911", 2, $"Dispatch", $"Assistance Requested", $"Officer: ~b~{playerName}~n~~s~Code: ~b~{code}", 2);
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"CHAR_CALL911", 2, $"Dispatch", $"Details ~b~{playerName}", $"~s~Loc: ~b~{streetName}~n~{codeDetails}", 2);
        }
    }
}
