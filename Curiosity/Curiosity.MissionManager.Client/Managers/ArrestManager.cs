using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Events;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.MissionManager.Client.Manager
{
    public class ArrestManager : Manager<ArrestManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("job:police:arrest", new EventCallback(metadata =>
            {
                List<Ped> peds = World.GetAllPeds().Where(x => 
                    x.IsInRangeOf(Game.PlayerPed.Position, 20f)
                    && Decorators.GetBoolean(x.Handle, Decorators.PED_ARRESTED)
                    && Decorators.GetBoolean(x.Handle, Decorators.PED_HANDCUFFED)
                    ).Select(p => p).ToList();

                if (peds.Count == 0)
                {
                    Screen.ShowNotification("~b~Arrests: ~s~No suspect(s) to book found near by.");
                }

                peds.ForEach(p =>
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
                });

                return null;
            }));
        }
    }
}
