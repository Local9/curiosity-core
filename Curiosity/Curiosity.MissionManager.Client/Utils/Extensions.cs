using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.EventWrapperLegacy;
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



    }
}
