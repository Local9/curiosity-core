using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Managers
{
    class PlayerManager : BaseScript
    {
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public PlayerManager()
        {
            EventHandlers["curiosity:Client:Player:GetInformation"] += new Action<string>(OnPlayerInformation);
        }

        private void OnPlayerInformation(string json)
        {
            playerInfo = JsonConvert.DeserializeObject<PlayerInformationModel>(json);
        }

        public class PlayerInformationModel
        {
            public string Handle;
            public int UserId;
            public int CharacterId;
            public int RoleId;
            public int Wallet;
            public int BankAccount;
            public Dictionary<string, object> Skills;

            public bool IsDeveloper => RoleId == 4 || RoleId == 5;
        }
    }
}
