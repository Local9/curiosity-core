using CitizenFX.Core;
using System.Collections.Generic;

namespace Curiosity.Global.Shared.Entity.GamePed
{
    public class PedObject
    {
        public PedHash pedHash;
        public Vector3 spawnPosition;
        public List<PedWeapon> weapons;
        public VehicleHash vehicleHash;
        public int combatMovement;
        public Dictionary<int, bool> combatAttributes;
        public int combatAbility;
        public int armor;
        public PedBlip pedBlip;
    }

    public class PedBlip
    {
        public BlipSprite blipSprite;
        public BlipColor blipColor;
        public bool IsShortRange;
        public bool IsFriendly;
        public string Name;
    }

    public class PedWeapon
    {
        public WeaponHash weaponHash;
        public int ammo;
        public bool equipNow;
        public bool isAmmoLoaded;
    }
}
