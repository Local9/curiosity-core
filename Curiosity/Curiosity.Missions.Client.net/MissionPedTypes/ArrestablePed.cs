using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.NPCType;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;

namespace Curiosity.Missions.Client.net.MissionPedTypes
{
    internal class ArrestablePed : WorldPed
    {
        private readonly Ped Ped;

        public ArrestablePed(int handle) : base(handle)
        {
            Ped = this;

            if (Decorators.GetBoolean(Ped.Handle, Decorators.DECOR_PED_INFLUENCE_ALCOHOL)
                || Decorators.GetBoolean(Ped.Handle, Client.DECOR_NPC_DRUG_ALCOHOL))
            {
                Profile = new NpcArrestable(true, (int)Ped.Gender);
            }
            else
            {
                bool influence = (Client.Random.Next(30) >= 28);
                Profile = new NpcArrestable(influence, (int)Ped.Gender);
            }

            Decorators.Set(Ped.Handle, Client.DECOR_PED_MISSION, true);

            SetDrunkMovementSet();
        }

        async void SetDrunkMovementSet()
        {
            if (Profile.IsUnderAlcaholInfluence)
            {
                if (!API.HasAnimSetLoaded(Client.MOVEMENT_ANIMATION_SET_DRUNK))
                {
                    API.RequestAnimSet(Client.MOVEMENT_ANIMATION_SET_DRUNK);

                    while (!API.HasAnimSetLoaded(Client.MOVEMENT_ANIMATION_SET_DRUNK))
                    {
                        await Client.Delay(100);
                    }
                }
                Ped.MovementAnimationSet = Client.MOVEMENT_ANIMATION_SET_DRUNK;
            }
        }
    }
}
