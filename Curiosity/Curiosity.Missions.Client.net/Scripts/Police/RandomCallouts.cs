using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Utils;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Managers;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.Mission;
using Curiosity.Missions.Client.Scripts.PedCreators;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Police
{
    class RandomCallouts
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        static bool AreEventsActive = false;

        static Blip _location;
        static bool IsCalloutActive = false;

        static public void Init()
        {
            RegisterCommand("createfight", new Action<int, List<object>, string>(DevCreateFight), false);

            PluginInstance.RegisterEventHandler("curiosity:Client:Missions:RandomEventCompleted", new Action(OnRandomEventCompleted));
        }

        private static void OnRandomEventCompleted()
        {
            IsCalloutActive = false;
            AreEventsActive = false;
            PluginInstance.RegisterTickHandler(OnRandomEventHandler);
        }

        internal static void Setup()
        {
            if (RandomMissionHandler.JobName != "police")
            {
                AreEventsActive = false;
                PluginInstance.DeregisterTickHandler(OnRandomEventHandler);
                return;
            }

            if (AreEventsActive) return;
            AreEventsActive = true;

            // PluginInstance.RegisterTickHandler(OnRandomEventHandler);
        }

        internal static void Dispose()
        {
            AreEventsActive = false;
            PluginInstance.DeregisterTickHandler(OnRandomEventHandler);
        }

        static async Task OnRandomEventHandler()
        {
            await BaseScript.Delay(0);
            long gameTime = GetGameTimer();
            int delay = Utility.RANDOM.Next(3, 10);
            int minute = (1000 * 60);

            while ((GetGameTimer() - gameTime) < (delay * minute))
            {
                if (RandomMissionHandler.JobName != "police")
                {
                    AreEventsActive = false;
                    PluginInstance.DeregisterTickHandler(OnRandomEventHandler);
                    return;
                }

                await BaseScript.Delay(10000);
            }

            gameTime = GetGameTimer();

            while ((GetGameTimer() - gameTime) < 30000)
            {
                await BaseScript.Delay(0);
                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to accept event, ~INPUT_FRONTENDCANCEL~ to decline.");

                if (Game.IsControlPressed(0, Control.Pickup))
                {
                    if (!IsCalloutActive)
                    {
                        int randomRunner = Utility.RANDOM.Next(2);

                        Static.Relationships.SetupRelationShips();

                        if (randomRunner == 1)
                        {
                            CreateFight();
                        }
                    }

                    if (RandomMissionHandler.JobName != "police")
                    {
                        AreEventsActive = false;
                        PluginInstance.DeregisterTickHandler(OnRandomEventHandler);
                        return;
                    }

                    break;
                }

                if (Game.IsControlPressed(0, Control.FrontendCancel))
                {
                    SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Utility.RANDOM.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Utility.RANDOM.Next(1, 5)} RESIDENT/OUTRO_0{Utility.RANDOM.Next(1, 4)}");
                    return;
                }
            }
        }

        static void DevCreateFight(int playerHandle, List<object> arguments, string raw)
        {
            if (!PlayerManager.IsDeveloper) return;
            CreateFight(true);
        }

        static public async void CreateFight(bool developer = false)
        {
            PluginInstance.DeregisterTickHandler(OnRandomEventHandler);

            if (RandomMissionHandler.JobName != "police")
            {
                AreEventsActive = false;
                return;
            }

            if (_location != null)
            {
                if (_location.Exists())
                    _location.Delete();
            }

            int randomNode = Utility.RANDOM.Next(200, 300);

            if (developer)
                randomNode = Utility.RANDOM.Next(100, 150);

            Vector3 pos = Game.PlayerPed.Position;
            Vector3 outpos = new Vector3();
            if (GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, randomNode, ref outpos, 0, 0, 0))
            {
                PluginManager.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 2", $"Assault", "Citizens have reported a domestic.", 2);
                PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
                SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Utility.RANDOM.Next(1, 3)} WE_HAVE/WE_HAVE_0{Utility.RANDOM.Next(1, 3)} CRIMES/CRIME_ASSAULT_0{Utility.RANDOM.Next(1, 3)} UNITS_RESPOND/UNITS_RESPOND_CODE_02_0{Utility.RANDOM.Next(1, 3)} RESIDENT/OUTRO_0{Utility.RANDOM.Next(1, 4)}");

                Blip location = World.CreateBlip(outpos);
                location.ShowRoute = true;

                while (Game.PlayerPed.Position.Distance(location.Position) >= 250f)
                {
                    await BaseScript.Delay(10);
                }

                Array pedHashes = Enum.GetValues(typeof(PedHash));
                Model model1 = PedHash.Hillbilly01AMM; // (PedHash)pedHashes.GetValue(PluginInstance.Random.Next(pedHashes.Length));
                Model model2 = PedHash.Hillbilly02AMM; // (PedHash)pedHashes.GetValue(PluginInstance.Random.Next(pedHashes.Length));

                Vector3 safeCoord = World.GetSafeCoordForPed(outpos, true, 16);

                if (!safeCoord.IsZero)
                    outpos = safeCoord;

                Ped suspect1Ped = await PedCreator.CreatePedAtLocation(model1, outpos + new Vector3(0f, -5f, 0f), 0);
                Ped suspect2Ped = await PedCreator.CreatePedAtLocation(model2, outpos + new Vector3(0f, 5f, 0f), 180);

                SetPedCombatAbility(suspect1Ped.Handle, 100);
                SetPedCombatAbility(suspect2Ped.Handle, 100);

                SetPedCombatAttributes(suspect1Ped.Handle, 5, true);
                SetPedCombatAttributes(suspect2Ped.Handle, 5, true);

                SetEntityCanBeDamagedByRelationshipGroup(suspect1Ped.Handle, false, Static.Relationships.Fighter2Relationship.Hash);
                SetEntityCanBeDamagedByRelationshipGroup(suspect2Ped.Handle, false, Static.Relationships.Fighter1Relationship.Hash);

                TaskPutPedDirectlyIntoMelee(suspect1Ped.Handle, suspect2Ped.Handle, 0f, 0f, 0f, false);
                TaskPutPedDirectlyIntoMelee(suspect2Ped.Handle, suspect1Ped.Handle, 0f, 0f, 0f, false);

                while (Game.PlayerPed.Position.Distance(location.Position) >= 50f)
                {
                    await BaseScript.Delay(100);
                }

                InteractivePed suspect1 = InteractivePedCreator.Ped(suspect1Ped, relationshipGroup: Static.Relationships.Fighter1Relationship);
                InteractivePed suspect2 = InteractivePedCreator.Ped(suspect2Ped, relationshipGroup: Static.Relationships.Fighter2Relationship);

                if (location.Exists())
                    location.Delete();
            }
        }
    }
}
