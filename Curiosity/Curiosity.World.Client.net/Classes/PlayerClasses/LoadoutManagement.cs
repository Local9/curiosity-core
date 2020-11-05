using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.GameWorld.Client.net.Classes.PlayerClasses
{
    class LoadoutManagement
    {
        static Client client = Client.GetInstance();
        static WeaponHash currentWeapon = WeaponHash.Unarmed;
        protected static List<WeaponHash> RestrictedWeapons = new List<WeaponHash>()
        {
            //WeaponHash.SniperRifle,
            //WeaponHash.FireExtinguisher,
            WeaponHash.CompactGrenadeLauncher,
            //WeaponHash.Snowball,
            WeaponHash.VintagePistol,
            WeaponHash.CombatPDW,
            WeaponHash.HeavySniperMk2,
            WeaponHash.HeavySniper,
            WeaponHash.SweeperShotgun,
            WeaponHash.MicroSMG,
            WeaponHash.Wrench,
            //WeaponHash.Pistol,
            //WeaponHash.PumpShotgun,
            //WeaponHash.APPistol,
            WeaponHash.Ball,
            WeaponHash.Molotov,
            //WeaponHash.SMG,
            WeaponHash.StickyBomb,
            //WeaponHash.PetrolCan,
            //WeaponHash.StunGun,
            WeaponHash.StoneHatchet,
            WeaponHash.AssaultRifleMk2,
            WeaponHash.HeavyShotgun,
            WeaponHash.Minigun,
            WeaponHash.GolfClub,
            WeaponHash.RayCarbine,
            //WeaponHash.FlareGun,
            //WeaponHash.Flare,
            WeaponHash.GrenadeLauncherSmoke,
            WeaponHash.Hammer,
            WeaponHash.PumpShotgunMk2,
            //WeaponHash.CombatPistol,
            WeaponHash.Gusenberg,
            WeaponHash.CompactRifle,
            WeaponHash.HomingLauncher,
            //WeaponHash.Nightstick,
            WeaponHash.MarksmanRifleMk2,
            WeaponHash.Railgun,
            WeaponHash.SawnOffShotgun,
            WeaponHash.SMGMk2,
            WeaponHash.BullpupRifle,
            WeaponHash.Firework,
            WeaponHash.CombatMG,
            //WeaponHash.CarbineRifle,
            WeaponHash.Crowbar,
            WeaponHash.BullpupRifleMk2,
            WeaponHash.SNSPistolMk2,
            WeaponHash.Flashlight,
            WeaponHash.Dagger,
            WeaponHash.Grenade,
            WeaponHash.PoolCue,
            WeaponHash.Bat,
            WeaponHash.SpecialCarbineMk2,
            WeaponHash.DoubleAction,
            WeaponHash.Pistol50,
            WeaponHash.Knife,
            WeaponHash.MG,
            WeaponHash.BullpupShotgun,
            WeaponHash.BZGas,
            //WeaponHash.Unarmed,
            WeaponHash.GrenadeLauncher,
            WeaponHash.NightVision,
            WeaponHash.Musket,
            WeaponHash.ProximityMine,
            WeaponHash.AdvancedRifle,
            WeaponHash.RayPistol,
            WeaponHash.RPG,
            WeaponHash.RayMinigun,
            WeaponHash.PipeBomb,
            WeaponHash.MiniSMG,
            WeaponHash.SNSPistol,
            WeaponHash.PistolMk2,
            WeaponHash.AssaultRifle,
            WeaponHash.SpecialCarbine,
            WeaponHash.Revolver,
            WeaponHash.MarksmanRifle,
            WeaponHash.RevolverMk2,
            WeaponHash.BattleAxe,
            WeaponHash.HeavyPistol,
            WeaponHash.KnuckleDuster,
            WeaponHash.MachinePistol,
            WeaponHash.CombatMGMk2,
            WeaponHash.MarksmanPistol,
            WeaponHash.Machete,
            WeaponHash.SwitchBlade,
            WeaponHash.AssaultShotgun,
            WeaponHash.DoubleBarrelShotgun,
            WeaponHash.AssaultSMG,
            WeaponHash.Hatchet,
            WeaponHash.Bottle,
            WeaponHash.CarbineRifleMk2,
            WeaponHash.Parachute,
            //WeaponHash.SmokeGrenade,
        };

        static public void Init()
        {
            client.RegisterTickHandler(CheckCurrentWeaponIsAllowed);
        }

        static async Task CheckCurrentWeaponIsAllowed()
        {
            if (PlayerInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(CheckCurrentWeaponIsAllowed);
            }

            WeaponHash selectedWeapon = (WeaponHash)GetSelectedPedWeapon(Client.PedHandle);
            if (selectedWeapon != currentWeapon)
            {
                if (IsWeaponRestricted(selectedWeapon))
                {
                    Game.PlayerPed.Weapons.Remove(selectedWeapon);
                }
                else
                {
                    currentWeapon = selectedWeapon;
                }
            }
        }

        static private bool IsWeaponRestricted(WeaponHash weaponToCheck)
        {
            foreach (WeaponHash weapon in RestrictedWeapons)
            {
                return (weapon == weaponToCheck);
            }
            return false;
        }

        static public void StoreLoadout()
        {

        }
    }
}
