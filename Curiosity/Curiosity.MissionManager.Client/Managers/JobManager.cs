using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Handler;
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
        private const string JOB_POLICE = "police";
        internal static PatrolZone PatrolZone;
        internal static bool IsOnDuty;
        internal static bool IsOfficer;
        
        private static bool HasShownScaleform;
        private static Scaleform scaleform;

        public override void Begin()
        {
            Logger.Info($"------------- [JobManager] Begin -----------------");

            Instance.EventRegistry[LegacyEvents.Client.PoliceDutyEvent] += new Action<bool, bool, string>(OnJobDutyEvent);
            Instance.EventRegistry[LegacyEvents.Client.PolicePatrolZone] += new Action<int>(OnJobPatrolZone);

            EventSystem.Attach(LegacyEvents.Client.PolicePatrolZone, new EventCallback(metadata =>
            {
                PatrolZone = (PatrolZone)metadata.Find<int>(0);
                return null;
            }));
        }

        private void OnJobPatrolZone(int patrolZone)
        {
            Logger.Debug($"OnJobPatrolZone: {patrolZone}");

            PatrolZone = (PatrolZone)patrolZone;
        }

        private async void OnJobDutyEvent(bool active, bool onDuty, string job)
        {
            Logger.Debug($"OnJobDutyEvent: {job}:{onDuty}");

            IsOnDuty = onDuty;

            if (IsOfficer != (job == JOB_POLICE))
                HasShownScaleform = false;

            IsOfficer = (job == JOB_POLICE);

            if (IsOfficer)
            {
                if (!HasShownScaleform)
                    ShowScaleformRules();

                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Cop;

                MarkerHandler.Init();
                await BaseScript.Delay(100);
                MarkerArrest.Init();
            }
            else
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Player;

                MarkerHandler.Dispose();
                await BaseScript.Delay(100);
                MarkerArrest.Dispose();
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

            string description = "~w~Guides can be found via the Intraction Menu [~o~M~w~].\n";
            description += "~r~~h~DO NOT~h~~w~ Pull over other players.\n";
            description += "~r~~h~DO NOT~h~~w~ Drive recklessly.\n";
            description += "~r~~h~DO NOT~h~~w~ Force people to RolePlay, doing so will get you kicked.\n";
            description += "\n";
            description += "~g~To Get Started:\n";
            description += "~w~Open the Activity Menu [~o~F1~w~]\n";
            description += "~w~Here you will find all your options to your job.\n";
            description += "Finally, keep it friendly and we'll all get along.\n";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑\n";
            description += "~o~REMEMBER Press ~b~~h~M~h~~o~ to read the guides for more information!~w~\n";
            description += "~b~Forums~w~: forums.lifev.net / ~b~Discord~w~: discord.lifev.net";

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

            Screen.ShowNotification("~w~If you are stuck, use ~b~/stuck~w~ to respawn safely.");

            API.SetStreamedTextureDictAsNoLongerNeeded(dictTexture);
        }
    }
}
