using System.ComponentModel;

namespace Curiosity.Framework.Server.Models.Database
{
    public partial class Character
    {
        #region FIELDS
        [Description("characterId")]
        public int CharacterId { get; private set; }

        [Description("userId")]
        public int UserID { get; private set; }

        [Description("serverId")]
        public int ServerId { get; private set; }

        [Description("bankId")]
        public int BankId { get; private set; }

        [Description("Cash")]
        public ulong Cash { get; private set; }

        [Description("IsRegistered")]
        public int IsRegistered { get; private set; }

        [Description("CharacterJSON")]
        public string CharacterJson { get; private set; }
        #endregion
    }
}
