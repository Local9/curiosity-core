using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.NPCType;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Utils;

namespace Curiosity.Missions.Client.MissionPedTypes
{
    internal class ArrestablePed : WorldPed
    {
        private readonly Ped Ped;

        public ArrestablePed(int handle) : base(handle)
        {
            Ped = this;

            if (Decorators.GetBoolean(Ped.Handle, Decorators.PED_INFLUENCE_ALCOHOL)
                || Decorators.GetBoolean(Ped.Handle, Decorators.PED_INFLUENCE_DRUG))
            {
                Profile = new NpcArrestable(true, (int)Ped.Gender);
            }
            else
            {
                bool influence = (PluginManager.Random.Next(30) >= 28);
                Profile = new NpcArrestable(influence, (int)Ped.Gender);
            }

            Decorators.Set(Ped.Handle, Decorators.PED_MISSION, true);

            SetDrunkMovementSet();
        }

        async void SetDrunkMovementSet()
        {
            if (Profile.IsUnderAlcaholInfluence)
            {
                if (!API.HasAnimSetLoaded(PluginManager.MOVEMENT_ANIMATION_SET_DRUNK))
                {
                    API.RequestAnimSet(PluginManager.MOVEMENT_ANIMATION_SET_DRUNK);

                    while (!API.HasAnimSetLoaded(PluginManager.MOVEMENT_ANIMATION_SET_DRUNK))
                    {
                        await PluginManager.Delay(100);
                    }
                }
                Ped.MovementAnimationSet = PluginManager.MOVEMENT_ANIMATION_SET_DRUNK;
            }
        }
    }
}
