using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.EventWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Callouts.Client.Managers
{
    class PlayerManager : BaseScript
    {
        static ExportDictionary exportDictionary;
        static PlayerInformationModel playerInfo;
        static public PatrolZone PatrolZone = PatrolZone.City;
        public PlayerManager()
        {
            EventHandlers[Events.Client.ServerPlayerInformationUpdate] += new Action<string>(OnPlayerInformationUpdate);
            EventHandlers[Events.Client.ReceivePlayerInformation] += new Action<string>(OnPlayerInformationUpdate);
            EventHandlers[Events.Client.PolicePatrolZone] += new Action<int>(OnPlayerPatrolZone);
        }

        private void OnPlayerPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
        }

        private void OnPlayerInformationUpdate(string json)
        {
            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(json);
        }

        public static bool IsDeveloper => playerInfo.RoleId == 4 || playerInfo.RoleId == 5;

        public class PlayerInformationModel
        {
            public string Handle;
            public int UserId;
            public int CharacterId;
            public int RoleId;
            public int Wallet;
            public int BankAccount;
            public Dictionary<string, Skills> Skills;
        }

        public class Skills
        {
            public int Id;
            public int TypeId;
            public string Description;
            public string Label;
            public string LabelDescription;
            public int Value;
        }
    }
}
