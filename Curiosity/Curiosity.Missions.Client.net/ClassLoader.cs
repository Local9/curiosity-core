using Curiosity.Shared.Client.net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Missions.Client.net
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
            DataClasses.Mission.PoliceStores.Init();

            Classes.Environment.ChatCommands.Init();

            // extras
            Scripts.Extras.Coroner.Init();
            Scripts.Extras.VehicleTow.Init();

            // MISSION HANDLER
            Scripts.Mission.RandomMissionHandler.Init();
            Scripts.MissionEvents.Init();

            Scripts.Menus.PedInteractionMenu.MenuBase.Init();
            client.RegisterTickHandler(OnCleanup);

            Log.Verbose("Leaving ClassLoader Init");
        }

        static async Task OnCleanup()
        {
            await Task.FromResult(0);
            List<Ped> peds = World.GetAllPeds().Where(x => x.Position.Distance(Game.PlayerPed.Position) < 20f).Select(p => p).ToList();

            peds.ForEach(async p =>
            {
                if (DecorExistOn(p.Handle, Client.NPC_WAS_RELEASED))
                {
                    if (DecorGetBool(p.Handle, Client.NPC_WAS_RELEASED))
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