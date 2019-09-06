using CitizenFX.Core;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Player
{
    class Weapons
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            // Need to block all guns, unless whitelisted
            client.RegisterTickHandler(OnTick);
        }

        static async Task OnTick()
        {
            if (!PlayerInformation.IsDeveloper())
            {
                if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Minigun))
                {
                    Game.PlayerPed.Weapons.RemoveAll();
                    Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
                }
            }
            await BaseScript.Delay(100);
            await Task.FromResult(0);
        }
    }
}
