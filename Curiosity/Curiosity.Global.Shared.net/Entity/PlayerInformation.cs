using System.Collections.Generic;

namespace Curiosity.Global.Shared.net.Entity
{
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
}
