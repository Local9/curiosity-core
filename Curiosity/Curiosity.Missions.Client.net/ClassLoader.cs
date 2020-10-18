using CitizenFX.Core;
using Curiosity.Missions.Client.Classes.Environment;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.DataClasses.Mission;
using Curiosity.Missions.Client.Scripts;
using Curiosity.Missions.Client.Scripts.Extras;
using Curiosity.Missions.Client.Scripts.Menus.PedInteractionMenu;
using Curiosity.Missions.Client.Scripts.Mission;
using Curiosity.Missions.Client.Scripts.Mission.PoliceMissions;
using Curiosity.Missions.Client.Scripts.Police;
using Curiosity.Missions.Client.Static;
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
        static PluginManager PluginInstance => PluginManager.Instance;

        public static void Init()
        {
            Log.Verbose("Entering ClassLoader Init");

            ClientInformation.Init();

            // Game Code
            Relationships.Init();

            // DATA
            PoliceCallouts.Init();

            ChatCommands.Init();

            // extras
            Coroner.Init();
            VehicleTow.Init();

            // MISSION HANDLER
            RandomCallouts.Init();
            RandomMissionHandler.Init();
            MissionEvents.Init();
            NpcHandler.Init();

            MenuBase.Init();
            PluginInstance.RegisterTickHandler(OnCleanup);

            HumainLabs.Init();

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
                if (DecorExistOn(p.Handle, PluginManager.DECOR_NPC_WAS_RELEASED))
                {
                    if (DecorGetBool(p.Handle, PluginManager.DECOR_NPC_WAS_RELEASED))
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