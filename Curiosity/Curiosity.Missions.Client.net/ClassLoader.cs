using CitizenFX.Core;
using Curiosity.Missions.Client.Classes.Environment;
using Curiosity.Missions.Client.DataClasses.Mission;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client
{
    /// <summary>
    /// For initialization of all these static classes
    /// May want to split this into multiple more specific files later
    /// </summary>
    static class ClassLoader
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            Classes.PlayerClient.ClientInformation.Init();

            // Game Code
            Static.Relationships.Init();

            // DATA
            DataClasses.Mission.PoliceCallouts.Init();

            Classes.Environment.ChatCommands.Init();

            // extras
            Scripts.Extras.Coroner.Init();
            Scripts.Extras.VehicleTow.Init();

            // MISSION HANDLER
            Scripts.Police.RandomCallouts.Init();
            Scripts.Mission.RandomMissionHandler.Init();
            Scripts.MissionEvents.Init();
            Scripts.NpcHandler.Init();

            Scripts.Menus.PedInteractionMenu.MenuBase.Init();
            client.RegisterTickHandler(OnCleanup);

            Scripts.Mission.PoliceMissions.HumainLabs.Init();

            GameEvents.Init();
            GameEventHandlers.Init();

            PoliceMissionData.Init();

            Log.Verbose("Leaving ClassLoader Init");
        }

        static async Task OnCleanup()
        {
            await Task.FromResult(0);
            List<Ped> peds = World.GetAllPeds().Where(x => x.Position.Distance(Game.PlayerPed.Position) < 20f).Select(p => p).ToList();

            peds.ForEach(async p =>
            {
                if (DecorExistOn(p.Handle, Client.DECOR_NPC_WAS_RELEASED))
                {
                    if (DecorGetBool(p.Handle, Client.DECOR_NPC_WAS_RELEASED))
                    {
                        p.MarkAsNoLongerNeeded();
                        p.IsPersistent = false;
                        NetworkFadeOutEntity(p.Handle, true, false);
                        await BaseScript.Delay(2000);
                        p.Delete();
                    }

                }
            });
        }
    }
}