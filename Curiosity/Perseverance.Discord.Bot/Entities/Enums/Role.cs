using System.ComponentModel;

namespace Perseverance.Discord.Bot.Entities.Enums
{
    internal enum Role : int
    {
        [Description("Unknown")]
        UNDEFINED = 0,
        [Description("User")]
        USER = 1,
        [Description("Moderator")]
        MODERATOR = 2,
        [Description("Administrator")]
        ADMINISTRATOR = 3,
        [Description("Developer")]
        DEVELOPER = 4,
        [Description("Project Manager")]
        PROJECT_MANAGER = 5,
        [Description("Senior Admin")]
        SENIOR_ADMIN = 6,
        [Description("Helper")]
        HELPER = 7,
        [Description("Head Admin")]
        HEAD_ADMIN = 8,
        [Description("Life V: Life Time Supporter")]
        DONATOR_LIFE = 9,
        [Description("Community Manager")]
        COMMUNITY_MANAGER = 10,
        [Description("Life V: Supporter I")]
        DONATOR_LEVEL_1 = 11,
        [Description("Life V: Supporter II")]
        DONATOR_LEVEL_2 = 12,
        [Description("Life V: Supporter III")]
        DONATOR_LEVEL_3 = 13,
    }
}
