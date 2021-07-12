using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
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
                List<Ped> peds = World.GetAllPeds().Where(x => x.IsInRangeOf(Cache.PlayerPed.Position, 20f)).Select(p => p).ToList();

                int numberOfPedsToArrest = 0;

                foreach(Ped ped in peds)
                {
                    bool pedArrested = ped.State.Get(StateBagKey.PED_ARRESTED) ?? false;
                    bool pedHandcuffed = ped.State.Get(StateBagKey.PED_HANDCUFFED) ?? false;
                    int ownerId = ped.State.Get(StateBagKey.PLAYER_OWNER) ?? -1;

                    if (ownerId == -1)
                        continue;

                    bool isOwnedByPlayer = ownerId == Game.Player.ServerId;

                    if (pedArrested && pedHandcuffed && isOwnedByPlayer)
                        numberOfPedsToArrest++;
                }

                if (numberOfPedsToArrest == 0)
                {
                    Screen.ShowNotification("~b~Arrests: ~s~No suspect(s) to book found near by.");
                }

                peds.ForEach(p =>
                {
                    Mission.RegisteredPeds.ForEach(async ped =>
                    {
                        if (ped.Handle == p.Handle)
                        {
                            bool pedArrested = ped.State.Get(StateBagKey.PED_ARRESTED) ?? false;
                            bool pedHandcuffed = ped.State.Get(StateBagKey.PED_HANDCUFFED) ?? false;

                            int ownerId = ped.State.Get(StateBagKey.PLAYER_OWNER) ?? -1;

                            bool isOwnedByPlayer = ownerId == Game.Player.ServerId;

                            if (pedArrested && pedHandcuffed && isOwnedByPlayer)
                            {
                                ped.ArrestPed();
                                await BaseScript.Delay(100);
                                Mission.CountArrest();
                            }

                            await BaseScript.Delay(10);
                        };
                    });
                });

                return null;
            }));
        }
    }
}
