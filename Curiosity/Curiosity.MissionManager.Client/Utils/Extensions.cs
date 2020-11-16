using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Utils
{
    public static class Extensions
    {
        public static void TriggerServer(this Event @event) =>
            BaseScript.TriggerServerEvent(@event.Path);

        public static void TriggerServer<T1>(this Event<T1> @event, T1 field1) =>
            BaseScript.TriggerServerEvent(@event.Path, field1);

        public static void TriggerServer<T1, T2>(this LegacyEvent<T1, T2> @event, T1 field1, T2 field2) =>
            BaseScript.TriggerServerEvent(@event.Path, field1, field2);

        public static void TriggerServer<T1, T2, T3>(this Event<T1, T2, T3> @event, T1 field1, T2 field2, T3 field3) =>
            BaseScript.TriggerServerEvent(@event.Path, field1, field2, field3);

        public static void TriggerServer<T1, T2, T3, T4>(this Event<T1, T2, T3, T4> @event, T1 field1, T2 field2,
            T3 field3,
            T4 field4) =>
            BaseScript.TriggerServerEvent(@event.Path, field1, field2, field3, field4);

        public static bool IsPlayingAnim(this Entity entity, string animSet, string animName)
        {
            return API.IsEntityPlayingAnim(entity.Handle, animSet, animName, 3);
        }

        public static float VDist(this Vector3 v, Vector3 to)
        {
            return API.Vdist(v.X, v.Y, v.Z, to.X, to.Y, to.Z);
        }

        public static float DistanceTo(this Vector3 position, Vector3 target) => (position - target).Length();

        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }

        public async static Task FadeOut(this Ped ped, bool slow = false)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = false)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = false)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(ped.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            await BaseScript.Delay(3000);
        }

        public async static Task FadeOut(this Vehicle vehicle, bool slow = false)
        {
            await Fade(vehicle, false, slow);
        }

        public async static Task FadeIn(this Vehicle vehicle, bool slow = false)
        {
            await Fade(vehicle, true, slow);
        }

        public async static Task Fade(this Vehicle vehicle, bool fadeIn, bool slow = false)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(vehicle.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(vehicle.Handle, false, slow);
            }

            await BaseScript.Delay(3000);
        }


    }
}
