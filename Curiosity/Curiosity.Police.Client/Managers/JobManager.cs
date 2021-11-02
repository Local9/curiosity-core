using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;
using System.Threading.Tasks;
using Curiosity.Systems.Library.Enums;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Interface;

namespace Curiosity.Police.Client.Managers
{
    public class JobManager : Manager<JobManager>
    {
        private const string JOB_POLICE = "Police Officer";
        internal static bool IsOnDuty = false;
        internal static bool IsOfficer = false;
        internal static bool WasOfficer = false;

        internal static int isPassiveStateBagHandler = 0;

        public override void Begin()
        {
            EventSystem.Attach("job:police:duty", new EventCallback(metadata =>
            {
                Logger.Info($"OnJobDutyEvent");

                string job = IsOfficer ? "Unemployed" : JOB_POLICE;
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, true, false, job);
                return null;
            }));

            // LEGACY (rather not do this but I have to
            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnJobDutyEvent);
        }

        private void OnStatePlayerPassiveChange(string bag, string key, dynamic isPassive, int reserved, bool replicated)
        {
            if (IsOfficer && isPassive)
            {
                BaseScript.TriggerEvent(LegacyEvents.Client.CuriosityJob, false, false, "Unemployed");
                Notify.Warning($"Police Officers cannot have Passive Enabled, you have been removed from the force.");
            }
        }

        public async void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            bool isPassive = Game.Player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? true;

            Logger.Info($"OnJobDutyEvent: {job}:{onDuty}");

            if (isPassive && job == JOB_POLICE)
            {
                Notify.Warning($"Police Officers cannot join if Passive is Enabled.");
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
                Notify.Info($"Welcome to the force");
                Instance.AttachTickHandler(OnDisablePolice);

                isPassiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
            }
            else if (!IsOfficer && WasOfficer)
            {
                Instance.DetachTickHandler(OnDisablePolice);

                Game.PlayerPed.CanBeDraggedOutOfVehicle = true;

                WasOfficer = false;
                Instance.DiscordRichPresence.Status = "Roaming around";
                Instance.DiscordRichPresence.SmallAsset = "fivem";
                Instance.DiscordRichPresence.SmallAssetText = "FiveM";
                Instance.DiscordRichPresence.Commit();

                Notify.Info($"No longer a police officer");

                RemoveStateBagChangeHandler(isPassiveStateBagHandler);

                await BaseScript.Delay(100);
            }

            await BaseScript.Delay(100);
            EventSystem.Request<object>("user:job", job);
        }

        async Task OnDisablePolice()
        {
            SetMaxWantedLevel(0);
            await BaseScript.Delay(500);
        }
    }
}
