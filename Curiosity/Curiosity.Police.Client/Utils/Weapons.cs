﻿using CitizenFX.Core.Native;
using System.Linq;

namespace Curiosity.Police.Client.Utils
{
    public class Weapon
    {
        public Weapon(uint model, int ammo, int ammoInClip, uint[] components, int tintIndex)
        {
            Model = model;
            Ammo = ammo;
            AmmoInClip = ammoInClip;
            Components = components;
            TintIndex = tintIndex;
        }

        public uint Model { get; }
        public int Ammo { get; }
        public int AmmoInClip { get; }
        public uint[] Components { get; }
        public int TintIndex { get; }
    }

    public static class Weapons
    {
        private static readonly uint[] WeaponHashes;
        private static readonly uint[] ComponentHashes;

        static Weapons()
        {
            WeaponHashes = WeaponList.Select(weapon => (uint)API.GetHashKey(weapon)).ToArray();
            ComponentHashes = ComponentList.Select(component => (uint)API.GetHashKey(component)).ToArray();
        }

        public static Weapon Get(int ped, uint weapon)
        {
            var ammo = API.GetAmmoInPedWeapon(ped, weapon);
            int ammoInClip = API.GetMaxAmmoInClip(ped, weapon, false);
            API.GetAmmoInClip(ped, weapon, ref ammoInClip);
            var compoments = ComponentHashes.Where(component => API.DoesWeaponTakeWeaponComponent(weapon, component) && API.HasPedGotWeaponComponent(ped, weapon, component)).ToArray();
            var tintIndex = API.GetPedWeaponTintIndex(ped, weapon);

            return new Weapon(weapon, ammo, ammoInClip, compoments, tintIndex);
        }

        public static Weapon[] Get(int ped)
        {
            return WeaponHashes.Where(weapon => API.HasPedGotWeapon(ped, weapon, false)).Select(weapon => Get(ped, weapon)).ToArray();
        }

        public static void Give(int ped, uint weapon)
        {
            API.GiveWeaponToPed(ped, weapon, 1, false, true);

            int ammoInClip = API.GetMaxAmmoInClip(ped, weapon, false);
            API.SetAmmoInClip(ped, weapon, ammoInClip);

            var ammo = int.MaxValue;
            API.GetMaxAmmo(ped, weapon, ref ammo);
            API.SetPedAmmo(ped, weapon, ammo);
        }

        public static void Give(int ped, Weapon weapon)
        {
            API.GiveWeaponToPed(ped, weapon.Model, 1, false, true);
            API.SetAmmoInClip(ped, weapon.Model, weapon.AmmoInClip);
            API.SetPedAmmo(ped, weapon.Model, weapon.Ammo);
            API.SetPedWeaponTintIndex(ped, weapon.Model, weapon.TintIndex);

            foreach (var component in weapon.Components)
            {
                API.GiveWeaponComponentToPed(ped, weapon.Model, component);
            }
        }

        public static void Give(int ped, Weapon[] loadout)
        {
            foreach (var weapon in loadout)
            {
                Give(ped, weapon);
            }

            API.SetCurrentPedWeapon(ped, (uint)API.GetHashKey("weapon_unarmed"), true);
        }

        // source: https://github.com/TomGrobbe/vMenu/blob/master/vMenu/data/ValidWeapon.cs
        private static readonly string[] WeaponList = new string[]
        {
            "weapon_pocce_ropegun", // ropegun <3
            "weapon_advancedrifle",
            "weapon_appistol",
            "weapon_assaultrifle",
            "weapon_assaultrifle_mk2",
            "weapon_assaultshotgun",
            "weapon_assaultsmg",
            "weapon_autoshotgun",
            "weapon_ball",
            "weapon_bat",
            "weapon_battleaxe",
            "weapon_bottle",
            "weapon_bullpuprifle",
            "weapon_bullpuprifle_mk2",
            "weapon_bullpupshotgun",
            "weapon_bzgas",
            "weapon_carbinerifle",
            "weapon_carbinerifle_mk2",
            "weapon_combatmg",
            "weapon_combatmg_mk2",
            "weapon_combatpdw",
            "weapon_combatpistol",
            "weapon_compactlauncher",
            "weapon_compactrifle",
            "weapon_crowbar",
            "weapon_dagger",
            "weapon_dbshotgun",
            "weapon_doubleaction",
            "weapon_fireextinguisher",
            "weapon_firework",
            "weapon_flare",
            "weapon_flaregun",
            "weapon_flashlight",
            "weapon_golfclub",
            "weapon_grenade",
            "weapon_grenadelauncher",
            "weapon_gusenberg",
            "weapon_hammer",
            "weapon_hatchet",
            "weapon_heavypistol",
            "weapon_heavyshotgun",
            "weapon_heavysniper",
            "weapon_heavysniper_mk2",
            "weapon_hominglauncher",
            "weapon_knife",
            "weapon_knuckle",
            "weapon_machete",
            "weapon_machinepistol",
            "weapon_marksmanpistol",
            "weapon_marksmanrifle",
            "weapon_marksmanrifle_mk2",
            "weapon_mg",
            "weapon_microsmg",
            "weapon_minigun",
            "weapon_minismg",
            "weapon_molotov",
            "weapon_musket",
            "weapon_navyrevolver",
            "weapon_nightstick",
            "weapon_petrolcan",
            "weapon_pipebomb",
            "weapon_pistol",
            "weapon_pistol50",
            "weapon_pistol_mk2",
            "weapon_poolcue",
            "weapon_proxmine",
            "weapon_pumpshotgun",
            "weapon_pumpshotgun_mk2",
            "weapon_railgun",
            "weapon_raycarbine",
            "weapon_rayminigun",
            "weapon_raypistol",
            "weapon_revolver",
            "weapon_revolver_mk2",
            "weapon_rpg",
            "weapon_sawnoffshotgun",
            "weapon_smg",
            "weapon_smg_mk2",
            "weapon_smokegrenade",
            "weapon_sniperrifle",
            "weapon_snowball",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_specialcarbine",
            "weapon_specialcarbine_mk2",
            "weapon_stickybomb",
            "weapon_stone_hatchet",
            "weapon_stungun",
            "weapon_switchblade",
            "weapon_unarmed",
            "weapon_vintagepistol",
            "weapon_wrench"
        };

        private static readonly string[] ComponentList = new string[]
        {
            "COMPONENT_ADVANCEDRIFLE_CLIP_01",
            "COMPONENT_ADVANCEDRIFLE_CLIP_02",
            "COMPONENT_ADVANCEDRIFLE_VARMOD_LUXE",
            "COMPONENT_APPISTOL_CLIP_01",
            "COMPONENT_APPISTOL_CLIP_02",
            "COMPONENT_APPISTOL_VARMOD_LUXE",
            "COMPONENT_ASSAULTRIFLE_CLIP_01",
            "COMPONENT_ASSAULTRIFLE_CLIP_02",
            "COMPONENT_ASSAULTRIFLE_CLIP_03",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_02",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_03",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_04",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_05",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_06",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_07",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_08",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_09",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_10",
            "COMPONENT_ASSAULTRIFLE_MK2_CAMO_IND_01",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_01",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_02",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_FMJ",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_INCENDIARY",
            "COMPONENT_ASSAULTRIFLE_MK2_CLIP_TRACER",
            "COMPONENT_ASSAULTRIFLE_VARMOD_LUXE",
            "COMPONENT_ASSAULTSHOTGUN_CLIP_01",
            "COMPONENT_ASSAULTSHOTGUN_CLIP_02",
            "COMPONENT_ASSAULTSMG_CLIP_01",
            "COMPONENT_ASSAULTSMG_CLIP_02",
            "COMPONENT_ASSAULTSMG_VARMOD_LOWRIDER",
            "COMPONENT_AT_AR_AFGRIP",
            "COMPONENT_AT_AR_AFGRIP_02",
            "COMPONENT_AT_AR_BARREL_01",
            "COMPONENT_AT_AR_BARREL_02",
            "COMPONENT_AT_AR_FLSH",
            "COMPONENT_AT_AR_SUPP",
            "COMPONENT_AT_AR_SUPP_02",
            "COMPONENT_AT_BP_BARREL_01",
            "COMPONENT_AT_BP_BARREL_02",
            "COMPONENT_AT_CR_BARREL_01",
            "COMPONENT_AT_CR_BARREL_02",
            "COMPONENT_AT_MG_BARREL_01",
            "COMPONENT_AT_MG_BARREL_02",
            "COMPONENT_AT_MRFL_BARREL_01",
            "COMPONENT_AT_MRFL_BARREL_02",
            "COMPONENT_AT_MUZZLE_01",
            "COMPONENT_AT_MUZZLE_02",
            "COMPONENT_AT_MUZZLE_03",
            "COMPONENT_AT_MUZZLE_04",
            "COMPONENT_AT_MUZZLE_05",
            "COMPONENT_AT_MUZZLE_06",
            "COMPONENT_AT_MUZZLE_07",
            "COMPONENT_AT_MUZZLE_08",
            "COMPONENT_AT_MUZZLE_09",
            "COMPONENT_AT_PI_COMP",
            "COMPONENT_AT_PI_COMP_02",
            "COMPONENT_AT_PI_COMP_03",
            "COMPONENT_AT_PI_FLSH",
            "COMPONENT_AT_PI_FLSH_02",
            "COMPONENT_AT_PI_FLSH_03",
            "COMPONENT_AT_PI_RAIL",
            "COMPONENT_AT_PI_RAIL_02",
            "COMPONENT_AT_PI_SUPP",
            "COMPONENT_AT_PI_SUPP_02",
            "COMPONENT_AT_SB_BARREL_01",
            "COMPONENT_AT_SB_BARREL_02",
            "COMPONENT_AT_SCOPE_LARGE",
            "COMPONENT_AT_SCOPE_LARGE_FIXED_ZOOM",
            "COMPONENT_AT_SCOPE_LARGE_FIXED_ZOOM_MK2",
            "COMPONENT_AT_SCOPE_LARGE_MK2",
            "COMPONENT_AT_SCOPE_MACRO",
            "COMPONENT_AT_SCOPE_MACRO_02",
            "COMPONENT_AT_SCOPE_MACRO_02_MK2",
            "COMPONENT_AT_SCOPE_MACRO_02_SMG_MK2",
            "COMPONENT_AT_SCOPE_MACRO_MK2",
            "COMPONENT_AT_SCOPE_MAX",
            "COMPONENT_AT_SCOPE_MEDIUM",
            "COMPONENT_AT_SCOPE_MEDIUM_MK2",
            "COMPONENT_AT_SCOPE_NV",
            "COMPONENT_AT_SCOPE_SMALL",
            "COMPONENT_AT_SCOPE_SMALL_02",
            "COMPONENT_AT_SCOPE_SMALL_MK2",
            "COMPONENT_AT_SCOPE_SMALL_SMG_MK2",
            "COMPONENT_AT_SCOPE_THERMAL",
            "COMPONENT_AT_SC_BARREL_01",
            "COMPONENT_AT_SC_BARREL_02",
            "COMPONENT_AT_SIGHTS",
            "COMPONENT_AT_SIGHTS_SMG",
            "COMPONENT_AT_SR_BARREL_01",
            "COMPONENT_AT_SR_BARREL_02",
            "COMPONENT_AT_SR_SUPP",
            "COMPONENT_AT_SR_SUPP_03",
            "COMPONENT_BULLPUPRIFLE_CLIP_01",
            "COMPONENT_BULLPUPRIFLE_CLIP_02",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_02",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_03",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_04",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_05",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_06",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_07",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_08",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_09",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_10",
            "COMPONENT_BULLPUPRIFLE_MK2_CAMO_IND_01",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_01",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_02",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_FMJ",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_INCENDIARY",
            "COMPONENT_BULLPUPRIFLE_MK2_CLIP_TRACER",
            "COMPONENT_BULLPUPRIFLE_VARMOD_LOW",
            "COMPONENT_CARBINERIFLE_CLIP_01",
            "COMPONENT_CARBINERIFLE_CLIP_02",
            "COMPONENT_CARBINERIFLE_CLIP_03",
            "COMPONENT_CARBINERIFLE_MK2_CAMO",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_02",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_03",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_04",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_05",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_06",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_07",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_08",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_09",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_10",
            "COMPONENT_CARBINERIFLE_MK2_CAMO_IND_01",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_01",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_02",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_FMJ",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_INCENDIARY",
            "COMPONENT_CARBINERIFLE_MK2_CLIP_TRACER",
            "COMPONENT_CARBINERIFLE_VARMOD_LUXE",
            "COMPONENT_COMBATMG_CLIP_01",
            "COMPONENT_COMBATMG_CLIP_02",
            "COMPONENT_COMBATMG_MK2_CAMO",
            "COMPONENT_COMBATMG_MK2_CAMO_02",
            "COMPONENT_COMBATMG_MK2_CAMO_03",
            "COMPONENT_COMBATMG_MK2_CAMO_04",
            "COMPONENT_COMBATMG_MK2_CAMO_05",
            "COMPONENT_COMBATMG_MK2_CAMO_06",
            "COMPONENT_COMBATMG_MK2_CAMO_07",
            "COMPONENT_COMBATMG_MK2_CAMO_08",
            "COMPONENT_COMBATMG_MK2_CAMO_09",
            "COMPONENT_COMBATMG_MK2_CAMO_10",
            "COMPONENT_COMBATMG_MK2_CAMO_IND_01",
            "COMPONENT_COMBATMG_MK2_CLIP_01",
            "COMPONENT_COMBATMG_MK2_CLIP_02",
            "COMPONENT_COMBATMG_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_COMBATMG_MK2_CLIP_FMJ",
            "COMPONENT_COMBATMG_MK2_CLIP_INCENDIARY",
            "COMPONENT_COMBATMG_MK2_CLIP_TRACER",
            "COMPONENT_COMBATPDW_CLIP_01",
            "COMPONENT_COMBATPDW_CLIP_02",
            "COMPONENT_COMBATPDW_CLIP_03",
            "COMPONENT_COMBATPISTOL_CLIP_01",
            "COMPONENT_COMBATPISTOL_CLIP_02",
            "COMPONENT_COMBATPISTOL_VARMOD_LOWRIDER",
            "COMPONENT_COMPACTRIFLE_CLIP_01",
            "COMPONENT_COMPACTRIFLE_CLIP_02",
            "COMPONENT_COMPACTRIFLE_CLIP_03",
            "COMPONENT_GUSENBERG_CLIP_01",
            "COMPONENT_GUSENBERG_CLIP_02",
            "COMPONENT_HEAVYPISTOL_CLIP_01",
            "COMPONENT_HEAVYPISTOL_CLIP_02",
            "COMPONENT_HEAVYPISTOL_VARMOD_LUXE",
            "COMPONENT_HEAVYSHOTGUN_CLIP_01",
            "COMPONENT_HEAVYSHOTGUN_CLIP_02",
            "COMPONENT_HEAVYSHOTGUN_CLIP_03",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_02",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_03",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_04",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_05",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_06",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_07",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_08",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_09",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_10",
            "COMPONENT_HEAVYSNIPER_MK2_CAMO_IND_01",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_01",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_02",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_EXPLOSIVE",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_FMJ",
            "COMPONENT_HEAVYSNIPER_MK2_CLIP_INCENDIARY",
            "COMPONENT_KNUCKLE_VARMOD_BALLAS",
            "COMPONENT_KNUCKLE_VARMOD_BASE",
            "COMPONENT_KNUCKLE_VARMOD_DIAMOND",
            "COMPONENT_KNUCKLE_VARMOD_DOLLAR",
            "COMPONENT_KNUCKLE_VARMOD_HATE",
            "COMPONENT_KNUCKLE_VARMOD_KING",
            "COMPONENT_KNUCKLE_VARMOD_LOVE",
            "COMPONENT_KNUCKLE_VARMOD_PIMP",
            "COMPONENT_KNUCKLE_VARMOD_PLAYER",
            "COMPONENT_KNUCKLE_VARMOD_VAGOS",
            "COMPONENT_MACHINEPISTOL_CLIP_01",
            "COMPONENT_MACHINEPISTOL_CLIP_02",
            "COMPONENT_MACHINEPISTOL_CLIP_03",
            "COMPONENT_MARKSMANRIFLE_CLIP_01",
            "COMPONENT_MARKSMANRIFLE_CLIP_02",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_02",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_03",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_04",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_05",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_06",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_07",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_08",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_09",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_10",
            "COMPONENT_MARKSMANRIFLE_MK2_CAMO_IND_01",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_01",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_02",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_FMJ",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_INCENDIARY",
            "COMPONENT_MARKSMANRIFLE_MK2_CLIP_TRACER",
            "COMPONENT_MARKSMANRIFLE_VARMOD_LUXE",
            "COMPONENT_MG_CLIP_01",
            "COMPONENT_MG_CLIP_02",
            "COMPONENT_MG_VARMOD_LOWRIDER",
            "COMPONENT_MICROSMG_CLIP_01",
            "COMPONENT_MICROSMG_CLIP_02",
            "COMPONENT_MICROSMG_VARMOD_LUXE",
            "COMPONENT_MINISMG_CLIP_01",
            "COMPONENT_MINISMG_CLIP_02",
            "COMPONENT_PISTOL50_CLIP_01",
            "COMPONENT_PISTOL50_CLIP_02",
            "COMPONENT_PISTOL50_VARMOD_LUXE",
            "COMPONENT_PISTOL_CLIP_01",
            "COMPONENT_PISTOL_CLIP_02",
            "COMPONENT_PISTOL_MK2_CAMO",
            "COMPONENT_PISTOL_MK2_CAMO_02",
            "COMPONENT_PISTOL_MK2_CAMO_03",
            "COMPONENT_PISTOL_MK2_CAMO_04",
            "COMPONENT_PISTOL_MK2_CAMO_05",
            "COMPONENT_PISTOL_MK2_CAMO_06",
            "COMPONENT_PISTOL_MK2_CAMO_07",
            "COMPONENT_PISTOL_MK2_CAMO_08",
            "COMPONENT_PISTOL_MK2_CAMO_09",
            "COMPONENT_PISTOL_MK2_CAMO_10",
            "COMPONENT_PISTOL_MK2_CAMO_IND_01",
            "COMPONENT_PISTOL_MK2_CLIP_01",
            "COMPONENT_PISTOL_MK2_CLIP_02",
            "COMPONENT_PISTOL_MK2_CLIP_FMJ",
            "COMPONENT_PISTOL_MK2_CLIP_HOLLOWPOINT",
            "COMPONENT_PISTOL_MK2_CLIP_INCENDIARY",
            "COMPONENT_PISTOL_MK2_CLIP_TRACER",
            "COMPONENT_PISTOL_VARMOD_LUXE",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_02",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_03",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_04",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_05",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_06",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_07",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_08",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_09",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_10",
            "COMPONENT_PUMPSHOTGUN_MK2_CAMO_IND_01",
            "COMPONENT_PUMPSHOTGUN_MK2_CLIP_01",
            "COMPONENT_PUMPSHOTGUN_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_PUMPSHOTGUN_MK2_CLIP_EXPLOSIVE",
            "COMPONENT_PUMPSHOTGUN_MK2_CLIP_HOLLOWPOINT",
            "COMPONENT_PUMPSHOTGUN_MK2_CLIP_INCENDIARY",
            "COMPONENT_PUMPSHOTGUN_VARMOD_LOWRIDER",
            "COMPONENT_REVOLVER_MK2_CAMO",
            "COMPONENT_REVOLVER_MK2_CAMO_02",
            "COMPONENT_REVOLVER_MK2_CAMO_03",
            "COMPONENT_REVOLVER_MK2_CAMO_04",
            "COMPONENT_REVOLVER_MK2_CAMO_05",
            "COMPONENT_REVOLVER_MK2_CAMO_06",
            "COMPONENT_REVOLVER_MK2_CAMO_07",
            "COMPONENT_REVOLVER_MK2_CAMO_08",
            "COMPONENT_REVOLVER_MK2_CAMO_09",
            "COMPONENT_REVOLVER_MK2_CAMO_10",
            "COMPONENT_REVOLVER_MK2_CAMO_IND_01",
            "COMPONENT_REVOLVER_MK2_CLIP_01",
            "COMPONENT_REVOLVER_MK2_CLIP_FMJ",
            "COMPONENT_REVOLVER_MK2_CLIP_HOLLOWPOINT",
            "COMPONENT_REVOLVER_MK2_CLIP_INCENDIARY",
            "COMPONENT_REVOLVER_MK2_CLIP_TRACER",
            "COMPONENT_REVOLVER_VARMOD_BOSS",
            "COMPONENT_REVOLVER_VARMOD_GOON",
            "COMPONENT_SAWNOFFSHOTGUN_VARMOD_LUXE",
            "COMPONENT_SMG_CLIP_01",
            "COMPONENT_SMG_CLIP_02",
            "COMPONENT_SMG_CLIP_03",
            "COMPONENT_SMG_MK2_CAMO",
            "COMPONENT_SMG_MK2_CAMO_02",
            "COMPONENT_SMG_MK2_CAMO_03",
            "COMPONENT_SMG_MK2_CAMO_04",
            "COMPONENT_SMG_MK2_CAMO_05",
            "COMPONENT_SMG_MK2_CAMO_06",
            "COMPONENT_SMG_MK2_CAMO_07",
            "COMPONENT_SMG_MK2_CAMO_08",
            "COMPONENT_SMG_MK2_CAMO_09",
            "COMPONENT_SMG_MK2_CAMO_10",
            "COMPONENT_SMG_MK2_CAMO_IND_01",
            "COMPONENT_SMG_MK2_CLIP_01",
            "COMPONENT_SMG_MK2_CLIP_02",
            "COMPONENT_SMG_MK2_CLIP_FMJ",
            "COMPONENT_SMG_MK2_CLIP_HOLLOWPOINT",
            "COMPONENT_SMG_MK2_CLIP_INCENDIARY",
            "COMPONENT_SMG_MK2_CLIP_TRACER",
            "COMPONENT_SMG_VARMOD_LUXE",
            "COMPONENT_SNIPERRIFLE_VARMOD_LUXE",
            "COMPONENT_SNSPISTOL_CLIP_01",
            "COMPONENT_SNSPISTOL_CLIP_02",
            "COMPONENT_SNSPISTOL_MK2_CAMO",
            "COMPONENT_SNSPISTOL_MK2_CAMO_02",
            "COMPONENT_SNSPISTOL_MK2_CAMO_03",
            "COMPONENT_SNSPISTOL_MK2_CAMO_04",
            "COMPONENT_SNSPISTOL_MK2_CAMO_05",
            "COMPONENT_SNSPISTOL_MK2_CAMO_06",
            "COMPONENT_SNSPISTOL_MK2_CAMO_07",
            "COMPONENT_SNSPISTOL_MK2_CAMO_08",
            "COMPONENT_SNSPISTOL_MK2_CAMO_09",
            "COMPONENT_SNSPISTOL_MK2_CAMO_10",
            "COMPONENT_SNSPISTOL_MK2_CAMO_IND_01",
            "COMPONENT_SNSPISTOL_MK2_CLIP_01",
            "COMPONENT_SNSPISTOL_MK2_CLIP_02",
            "COMPONENT_SNSPISTOL_MK2_CLIP_FMJ",
            "COMPONENT_SNSPISTOL_MK2_CLIP_HOLLOWPOINT",
            "COMPONENT_SNSPISTOL_MK2_CLIP_INCENDIARY",
            "COMPONENT_SNSPISTOL_MK2_CLIP_TRACER",
            "COMPONENT_SNSPISTOL_VARMOD_LOWRIDER",
            "COMPONENT_SPECIALCARBINE_CLIP_01",
            "COMPONENT_SPECIALCARBINE_CLIP_02",
            "COMPONENT_SPECIALCARBINE_CLIP_03",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_02",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_03",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_04",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_05",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_06",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_07",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_08",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_09",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_10",
            "COMPONENT_SPECIALCARBINE_MK2_CAMO_IND_01",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_01",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_02",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_ARMORPIERCING",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_FMJ",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_INCENDIARY",
            "COMPONENT_SPECIALCARBINE_MK2_CLIP_TRACER",
            "COMPONENT_SPECIALCARBINE_VARMOD_LOWRIDER",
            "COMPONENT_SWITCHBLADE_VARMOD_BASE",
            "COMPONENT_SWITCHBLADE_VARMOD_VAR1",
            "COMPONENT_SWITCHBLADE_VARMOD_VAR2",
            "COMPONENT_VINTAGEPISTOL_CLIP_01",
            "COMPONENT_VINTAGEPISTOL_CLIP_02"
        };
    }
}
