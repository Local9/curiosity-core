using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class JobManager : Manager<JobManager>
    {
        private const string JOB_POLICE = "Police Officer [PvP]";
        internal bool IsOnDuty = false;
        internal bool IsOfficer = false;
        internal bool WasOfficer = false;
        bool isEnabled = true;

        internal static int isPassiveStateBagHandler = 0;
        internal static int jobStateBagHandler = 0;
        internal int stunGunHash => GetHashKey("WEAPON_STUNGUN");

        internal NotificationManager Notify => NotificationManager.GetModule();
        internal PlayerOptionsManager playerOptionsManager => PlayerOptionsManager.GetModule();

        public override void Begin()
        {
            EventSystem.Attach("job:unemployed", new EventCallback(metadata =>
            {
                Logger.Debug($"JobManager: job:unemployed");
                string job = "Unemployed";
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, false, false, job);
                return null;
            }));

            EventSystem.Attach("job:police:duty", new EventCallback(metadata =>
            {
                Logger.Debug($"JobManager: job:police:duty");
                string job = IsOfficer ? "Unemployed" : JOB_POLICE;
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, true, false, job);
                return null;
            }));

            // LEGACY (rather not do this but I have to
            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);
        }

        private void OnStatePlayerJobChange(string bag, string key, dynamic jobId, int reserved, bool replicated)
        {
            if (IsOfficer && jobId == 0)
            {
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, false, false, "Unemployed");
            }
        }

        private void OnStatePlayerPassiveChange(string bag, string key, dynamic isPassive, int reserved, bool replicated)
        {
            if (IsOfficer && isPassive)
            {
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, false, false, "Unemployed");
                Notify.Warn($"Police Officers cannot have Passive Enabled, you have been removed from the force.");
            }
        }

        public async void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            if (!isEnabled && job == JOB_POLICE)
            {
                Notify.Warn($"Sorry, currently this job is disabled. Press F5 for all other jobs.");
                return;
            }

            if (playerOptionsManager.IsWanted)
            {
                Notify.Warn($"You're currently wanted by police.");
                return;
            }

            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                if (vehicle.ClassType != VehicleClass.Emergency)
                {
                    Notify.Warn($"Cannot activate the Police job while using a non-emergency vehicle. Please leave the vehicle, and activate the job again and then re-enter the vehicle if you want to use a non-emergency vehicle.");
                    return;
                }
            }

            bool isPassive = playerOptionsManager.IsPassive;

            Logger.Debug($"OnJobDutyEvent: {job}:{onDuty}");

            if (isPassive && job == JOB_POLICE)
            {
                Notify.Warn($"Police Officers cannot join if Passive is Enabled.");
                return;
            }

            IsOnDuty = onDuty;

            IsOfficer = (job == JOB_POLICE);

            if (IsOfficer && !WasOfficer)
            {
                WasOfficer = true;

                SetMaxWantedLevel(0);
                Game.PlayerPed.CanBeDraggedOutOfVehicle = false;

                Instance.DiscordRichPresence.Status = "On Duty";
                Instance.DiscordRichPresence.SmallAsset = "police";
                Instance.DiscordRichPresence.SmallAssetText = "Police Officer";
                Instance.DiscordRichPresence.Commit();

                Game.PlayerPed.IsInvincible = false; // trip because of legacy fireman

                await BaseScript.Delay(100);
                Instance.AttachTickHandler(OnPoliceStunGunMonitor);
                Instance.AttachTickHandler(OnDisablePoliceAndDispatch);

                isPassiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
                jobStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_JOB, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerJobChange));

                ToggleDispatch(false);
            }
            else if (!IsOfficer && WasOfficer)
            {
                Instance.DetachTickHandler(OnPoliceStunGunMonitor);
                Instance.DetachTickHandler(OnDisablePoliceAndDispatch);

                Game.PlayerPed.CanBeDraggedOutOfVehicle = true;

                WasOfficer = false;
                Instance.DiscordRichPresence.Status = "Roaming around";
                Instance.DiscordRichPresence.SmallAsset = "fivem";
                Instance.DiscordRichPresence.SmallAssetText = "FiveM";
                Instance.DiscordRichPresence.Commit();
                RemoveStateBagChangeHandler(isPassiveStateBagHandler);
                RemoveStateBagChangeHandler(jobStateBagHandler);

                ToggleDispatch(false);

                await BaseScript.Delay(100);
            }
            await BaseScript.Delay(100);
            EventSystem.Send("police:job:state", IsOfficer);
            await BaseScript.Delay(100);
            EventSystem.Send("user:job", job);
        }

        async Task OnDisablePoliceAndDispatch()
        {
            SetMaxWantedLevel(0);
            await BaseScript.Delay(500);
        }

        async Task OnPoliceStunGunMonitor()
        {
            bool isStungun = Game.PlayerPed.Weapons.Current.Hash == WeaponHash.StunGun;
            if (!isStungun)
            {
                await BaseScript.Delay(500);
                return;
            }

            if (Game.PlayerPed.IsShooting)
            {
                Vector3 weaponImpact = Game.PlayerPed.GetLastWeaponImpactPosition();
                List<Player> players = PluginManager.Instance.PlayerList.Where(p => p.Character.IsInRangeOf(weaponImpact, 1f)).ToList();
                foreach (Player player in players)
                {
                    bool isWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                    if (!isWanted)
                    {
                        EventSystem.Send("police:officerTazedPlayer", Game.Player.ServerId, player.ServerId);
                    }
                }
            }
        }

        void ToggleDispatch(bool toggle)
        {
            for (int i = 0; i < Dispatch.PoliceForces.Length; i++)
            {
                EnableDispatchService((int)Dispatch.PoliceForces[i], toggle);
            }
        }
    }
}
