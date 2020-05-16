using CitizenFX.Core.Native;
using Curiosity.System.Library.Inventory;
using System.Collections.Generic;

namespace Curiosity.System.Client.Inventory
{
    public class WeaponItem : InventoryItem
    {
        public bool IsEquipped { get; set; }

        public WeaponItem(string name, string label, string description) : base(name, label, description, false)
        {
        }

        public uint GetHash()
        {
            return (uint)API.GetHashKey(Name.ToUpper().Replace("::", "_"));
        }

        public int GetRealAmmunition()
        {
            var ammo = 0;

            API.GetAmmoInClip(Cache.Entity.Id, GetHash(), ref ammo);


            return API.GetAmmoInPedWeapon(Cache.Entity.Id, GetHash()) + ammo;
        }

        public int GetAmmunition()
        {
            if (Metadata == null) Metadata = new Dictionary<string, object>();
            if (!Metadata.ContainsKey("Weapon.Ammo")) UpdateAmmunition();

            return int.Parse(Metadata["Weapon.Ammo"].ToString());
        }

        public void UpdateAmmunition()
        {
            if (Metadata == null) Metadata = new Dictionary<string, object>();
            if (GetAmmunition() == GetRealAmmunition()) return;

            Metadata["Weapon.Ammo"] = GetRealAmmunition();

            this.GetParent().Commit();
        }

        public void Equip()
        {
            var ped = Cache.Entity.Id;
            var hash = GetHash();

            API.RemoveWeaponFromPed(ped, hash);
            API.GiveWeaponToPed(ped, hash, 0, false, false);
            API.SetPedAmmo(ped, hash, GetAmmunition());
            API.SetPedCanSwitchWeapon(ped, false);
            API.SetCurrentPedWeapon(ped, hash, true);

            IsEquipped = true;
        }

        public void Unequip()
        {
            var ped = Cache.Entity.Id;

            API.SetPedCanSwitchWeapon(ped, true);
            API.RemoveWeaponFromPed(ped, GetHash());
            API.SetCurrentPedWeapon(ped, (uint)API.GetHashKey("WEAPON_UNARMED"), true);

            IsEquipped = false;
        }
    }
}