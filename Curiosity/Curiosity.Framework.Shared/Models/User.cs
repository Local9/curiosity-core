using System.Collections.Generic;
using System.ComponentModel;

namespace Curiosity.Framework.Shared.Models
{
    public class User
    {
        #region FIELDS
        [Description("userId")]
        public int UserID { get; private set; }

        [Description("username")]
        public string Username { get; private set; }

        [Description("license")]
        public string License { get; private set; }

        [Description("discordId")]
        public ulong DiscordId { get; private set; }

        [Description("dateCreated")]
        public DateTime DateCreated { get; private set; }

        [Description("lastSeen")]
        public DateTime LastSeen { get; private set; }

        [Description("bannedPerm")]
        public int BannedPermanently { get; private set; }

        [Description("roleId")]
        public int RoleId { get; private set; }

        [Description("isPassive")]
        public int IsPassive { get; private set; }

        #endregion

        public List<Character> Characters { get; private set; } = new();
    }
}
