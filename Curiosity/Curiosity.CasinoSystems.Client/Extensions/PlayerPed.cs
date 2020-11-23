using CitizenFX.Core;

namespace Curiosity.CasinoSystems.Client.Extensions
{
    static class PlayerPed
    {
        public static bool IsFemale(this Ped ped)
        {
            return ped.Gender == Gender.Female;
        }
    }
}
