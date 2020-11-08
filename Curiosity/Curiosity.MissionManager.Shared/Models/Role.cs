using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public enum Role
    {
        UNDEFINED = 0,
        USER = 1,
        MODERATOR = 2,
        ADMINISTRATOR = 3,
        DEVELOPER = 4,
        PROJECT_MANAGER = 5,
        SENIOR_ADMIN = 6,
        HELPER = 7,
        HEAD_ADMIN = 8,
        DONATOR_LIFE = 9,
        COMMUNITY_MANAGER = 10,
        DONATOR_LEVEL_1 = 11,
        DONATOR_LEVEL_2 = 12,
        DONATOR_LEVEL_3 = 13,
    }

    public class Roles
    {
        public const ulong NITRO = 712753920123863170;
        public const ulong RESPECTED = 699316514201010298;
        public const ulong VETERAN = 665496068431151125;
        public const ulong EARLY_ACCESS = 607265838147567658;

        public Dictionary<ulong, Role> RoleDict = new Dictionary<ulong, Role>()
        {
            { 541955860344340490, Role.MODERATOR },
            { 541955955911557125, Role.ADMINISTRATOR },
            { 541956177085333514, Role.DEVELOPER },
            { 560446758854066187, Role.PROJECT_MANAGER },
            { 541956055958028298, Role.SENIOR_ADMIN },
            { 559081233511088140, Role.HELPER },
            { 568072517311397889, Role.HEAD_ADMIN },
            { 541955570601558036, Role.DONATOR_LIFE },
            { 588443443496222720, Role.DONATOR_LEVEL_1 },
            { 588440994543042560, Role.DONATOR_LEVEL_2 },
            { 588444129722105856, Role.DONATOR_LEVEL_3 },
        };
    }
}