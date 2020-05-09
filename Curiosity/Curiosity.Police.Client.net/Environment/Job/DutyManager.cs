using System;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace Curiosity.Police.Client.net.Environment.Job
{
    class DutyManager
    {
        static Client client = Client.GetInstance();

        public static bool IsOnDuty = false;
        public static bool IsPoliceJobActive = false;
        public static bool IsOnCallout = false;
        public static PatrolZone PatrolZone = PatrolZone.City;

        public static bool IsBirthday = false;

        private static bool RulesDisplayed = false;

        public static RelationshipGroup PoliceRelationshipGroup;

        static Scaleform scaleform;

        public static void Init()
        {
            PoliceRelationshipGroup = World.AddRelationshipGroup("POLICE_CALLOUTS");

            API.RegisterCommand("duty", new Action(OnPoliceDuty), false);

            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));

            client.RegisterEventHandler("curiosity:client:special", new Action<bool>(OnSpecialDay));
        }

        private static void OnSpecialDay(bool isBirthday)
        {
            IsBirthday = isBirthday;
        }

        static void OnPoliceDuty()
        {
            if (!Classes.Player.PlayerInformation.IsDeveloper()) return;

            Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "police");
        }

        static void OnDutyState(bool jobActive, bool dutyState, string job)
        {
            if (Classes.Player.PlayerInformation.IsDeveloper())
            {
                Log.Info($"OnDutyState -> Method Called");
            }

            if (job == "error")
            {
                Vehicle.LoadoutPosition.Dispose();
                Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Unknown);
                client.DeregisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);
                Game.PlayerPed.Weapons.RemoveAll();

                Log.Error("OnDutyState -> Error");
                IsPoliceJobActive = false;
                IsOnDuty = false;
                IsOnCallout = false;
                // Tasks.CalloutHandler.PlayerIsOnActiveCalloutOrOffDuty();
                return;
            }

            if (job != "police")
            {
                Vehicle.LoadoutPosition.Dispose();
                Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Unknown);
                client.DeregisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);

                Game.PlayerPed.Weapons.RemoveAll();

                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"OnDutyState -> Job changed from police to {job}");
                }
                IsPoliceJobActive = false;
                IsOnDuty = false;
                IsOnCallout = false;
                return; // TODO: Refactor job code
            }


            if (job == "police")
            {
                if (!RulesDisplayed)
                    ShowScaleformRules();

                Client.TriggerServerEvent("curiosity:Server:Player:Job", (int)Curiosity.Global.Shared.net.Enums.Job.Police);
                Game.PlayerPed.IsInvincible = false;
                Client.TriggerServerEvent("curiosity:client:special");

                client.RegisterTickHandler(Classes.Menus.PoliceDispatchMenu.OnTaskKeyCombination);

                IsOnDuty = dutyState;

                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"OnDutyState -> Player Duty State {IsOnDuty}");
                }
                Game.PlayerPed.RelationshipGroup = PoliceRelationshipGroup;
                Game.PlayerPed.DropsWeaponsOnDeath = false;
                Vehicle.LoadoutPosition.Init();
                IsPoliceJobActive = true;
            }
        }

        static async void ShowScaleformRules()
        {
            RulesDisplayed = true;
            ScaleformTask();
            await Client.Delay(10000);
            scaleform.Dispose();
        }

        static async void LoadDict(string dict)
        {
            API.RequestStreamedTextureDict(dict, false);
            while (!API.HasStreamedTextureDictLoaded(dict))
            {
                await Client.Delay(0);
            }
        }

        private static async void ScaleformTask()
        {
            scaleform = new Scaleform("GTAV_ONLINE");

            while (!scaleform.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            string description = "~w~Guides can be found via the Intraction Menu [M].~w~\n";
            description += "~r~DO NOT~w~ Pull over other players.\n";
            description += "~r~DO NOT~w~ Drive recklessly.\n";
            description += "~r~DO NOT~w~ Force people to RP.\n";
            description += "\n";
            description += "~g~To Get Started:\n";
            description += "~w~Open the Interaction Menu [M] and open Police Options\n";
            description += "~w~Here you will find all your options.\n";
            description += "~w~You can also press [LALT] + [E] to open the Back Up menu.\n";
            description += "Finally, keep it friendly and we'll all get along.\n";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑~b~∑~g~∑~y~∑~p~∑~o~∑";
            description += "~r~∑\n";
            description += "~o~REMEMBER Press ~b~M~o~ to read the guides for more information!~w~\n";
            description += "~b~Forums~w~: forums.lifev.net / ~b~Discord~w~: discord.lifev.net";

            scaleform.CallFunction("SETUP_TABS", 1, false);
            const string dictTexture = "www_arenawar_tv";
            LoadDict(dictTexture);
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

        static void OnPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Patrol Zone Change", $"Zone: {PatrolZone}", string.Empty, 2);
        }

        public static void OnSetCallOutStatus(bool status)
        {
            if (!IsPoliceJobActive)
            {
                IsOnCallout = false;
                return;
            }

            IsOnCallout = status;
        }

        static void OnGetCallOutStatus()
        {
            if (!IsPoliceJobActive) return;
            Client.TriggerEvent("curiosity:Client:Police:CallOutStatus", IsOnCallout);
        }
    }
}
