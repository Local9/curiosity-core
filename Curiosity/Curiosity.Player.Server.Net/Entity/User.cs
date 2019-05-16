using CitizenFX.Core;

namespace Curiosity.Server.net.Entity
{
    public class User
    {
        public long UserId;
        public long LocationId;
        public int RoleId;
        public string Role;
        public long WorldExperience;

        public int Wallet = 0;
        public int BankAccount = 0;
    }
}
