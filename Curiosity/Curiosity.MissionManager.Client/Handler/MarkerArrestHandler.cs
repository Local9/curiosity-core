using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    internal class MarkerArrestHandler
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        internal static void Init()
        {
        }

        private async static Task OnArrestPedTick()
        {
            Marker activeMarker = MarkerManager.GetActiveMarker(MarkerFilter.PoliceArrest);

            if (activeMarker == null)
            {
                await BaseScript.Delay(1000);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to book the ~b~suspect(s)~s~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                List<Ped> peds = World.GetAllPeds().Where(x => x.IsInRangeOf(activeMarker.Position, 20f)).Select(p => p).ToList();

                if (peds.Count == 0)
                {
                    Screen.ShowNotification("~b~Arrests: ~s~No suspect(s) to book found near by.");
                }

                peds.ForEach(p =>
                {
                    bool arrested = Decorators.GetBoolean(p.Handle, Decorators.PED_ARRESTED) && Decorators.GetBoolean(p.Handle, Decorators.PED_HANDCUFFED);

                    if (arrested)
                    {
                        Mission.RegisteredPeds.ForEach(async ped =>
                        {
                            if (ped.Handle == p.Handle)
                            {
                                ped.ArrestPed();
                                await BaseScript.Delay(100);
                                Mission.CountArrest();
                            };
                        });
                    }
                });

                await PluginManager.Delay(5000);
            }
        }
    }
}
