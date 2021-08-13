﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class JobManager : Manager<JobManager>
    {
        private const string JOB_POLICE = "policeOfficer";
        internal static PatrolZone PatrolZone = PatrolZone.City;
        internal static bool IsOnDuty;
        internal static bool IsOfficer;
        internal static bool WasOfficer;

        private static bool HasShownScaleform;
        private static Scaleform scaleform;

        public override void Begin()
        {
            Logger.Info($"- [JobManager] Begin -----------------------------");

            EventSystem.Attach("job:police:duty", new AsyncEventCallback(async metadata =>
            {
                string job = IsOfficer ? "unemployed" : JOB_POLICE;
                OnJobDutyEvent(true, false, job);
                await BaseScript.Delay(100);
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, true, false, job); // for legacy resources
                return null;
            }));

            // LEGACY
            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);
        }

        public async void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            Logger.Debug($"OnJobDutyEvent: {job}:{onDuty}");

            IsOnDuty = onDuty;

            if (IsOfficer != (job == JOB_POLICE))
                HasShownScaleform = false;

            IsOfficer = (job == JOB_POLICE);

            if (IsOfficer && !WasOfficer)
            {
                if (!HasShownScaleform)
                    ShowScaleformRules();

                WasOfficer = true;

                Instance.DiscordRichPresence.Status = "On Duty";
                Instance.DiscordRichPresence.SmallAsset = "police";
                Instance.DiscordRichPresence.SmallAssetText = "Police Officer";
                Instance.DiscordRichPresence.Commit();

                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Cop;
                Game.PlayerPed.IsInvincible = false; // trip because of legacy fireman

                Game.PlayerPed.Armor = 100;

                //await EquipWeapon(WeaponHash.Nightstick);
                //await EquipWeapon(WeaponHash.StunGun);
                //await EquipWeapon(WeaponHash.Flashlight);
                //await EquipWeapon(WeaponHash.Pistol, 250, true, true);

                await BaseScript.Delay(100);
                WorldVehicleManager.VehicleManager.Start();
            }
            else if (!IsOfficer && WasOfficer)
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Player;

                WasOfficer = false;
                Instance.DiscordRichPresence.Status = "Roaming around";
                Instance.DiscordRichPresence.SmallAsset = "fivem";
                Instance.DiscordRichPresence.SmallAssetText = "FiveM";
                Instance.DiscordRichPresence.Commit();

                MissionDirectorManager.Director.TurnOffMissionDirector();
                WorldVehicleManager.VehicleManager.Stop();

                await BaseScript.Delay(100);
            }

            Instance.ExportRegistry["curiosity-client"].SetJobActivity(active, onDuty, job);
            await BaseScript.Delay(100);
            EventSystem.Request<object>("user:job", job);
        }

        async Task EquipWeapon(WeaponHash weapon, int ammo = 1, bool equip = false, bool forceInHand = false)
        {
            await EventSystem.Request<bool>("character:weapons:equip", (uint)weapon, ammo, equip, forceInHand);

            bool hasWeapon = Game.PlayerPed.Weapons.HasWeapon(weapon);

            int failRate = 0;

            while (!hasWeapon)
            {
                await BaseScript.Delay(250);
                await EventSystem.Request<bool>("character:weapons:equip", (uint)weapon, ammo, equip, forceInHand);

                hasWeapon = Game.PlayerPed.Weapons.HasWeapon(weapon);

                if (!hasWeapon && failRate >= 5) break;

                failRate++;
            }
        }

        static async void ShowScaleformRules()
        {
            HasShownScaleform = true;
            ScaleformTask();
            await BaseScript.Delay(10000);
            scaleform.Dispose();
        }

        static async Task LoadDict(string dict)
        {
            API.RequestStreamedTextureDict(dict, false);

            int pings = 0;

            while (!API.HasStreamedTextureDictLoaded(dict) && pings < 10)
            {
                await BaseScript.Delay(100);
                API.RequestStreamedTextureDict(dict, false);
                pings++;
            }
        }

        private static async void ScaleformTask()
        {
            scaleform = new Scaleform("GTAV_ONLINE");

            while (!scaleform.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            string description = "~w~Guides can be found via the Tablet [~o~F11/HOME~w~].\n";
            description += "~r~~h~DO NOT~h~~w~ Pull over other players.\n";
            description += "~r~~h~DO NOT~h~~w~ Drive recklessly.\n";
            description += "~r~~h~DO NOT~h~~w~ Force people to RolePlay, doing so will get you kicked.\n";
            description += "~r~~h~DO NOT~h~~w~ Give us a reason to add another rule!\n";
            description += "\n";
            description += "~g~To Get Started:\n";
            description += "~w~Spawn a ~b~car~w~ at the garage, open the Activity Menu [~o~F1~w~]\n";
            description += "~w~Here you will find all your options to your job.\n";
            description += "Finally, keep it friendly and we'll all get along.\n";
            description += "\n";
            description += "You've been given some basic tools to get you started.\n";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑\n";
            description += "~o~REMEMBER Press ~b~~h~F11/HOME~h~~o~ to read the guides for more information!~w~\n";

            scaleform.CallFunction("SETUP_TABS", 1, false);
            const string dictTexture = "www_arenawar_tv";
            await LoadDict(dictTexture);
            scaleform.CallFunction("SETUP_TABS", true);
            scaleform.CallFunction("SET_BIGFEED_INFO", "Hello", description, 0, dictTexture, "bg_top_left", $"~y~Police Duty", "deprecated", $"Police Duty in Life V!", 0);
            scaleform.CallFunction("SET_NEWS_CONTEXT", 0);

            while (scaleform.IsLoaded)
            {
                await BaseScript.Delay(0);
                scaleform.Render2D();
            }

            API.SetStreamedTextureDictAsNoLongerNeeded(dictTexture);
        }
    }
}
