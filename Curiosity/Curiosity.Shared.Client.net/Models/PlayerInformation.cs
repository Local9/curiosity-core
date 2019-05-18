using System.Collections.Generic;

namespace Curiosity.Shared.Client.net.Models
{
    public class PlayerInformationModel
    {
        public string Handle;
        public long UserId;
        public int RoleId;
        public int Wallet;
        public int BankAccount;
        public Dictionary<string, int> Skills;
    }
}
