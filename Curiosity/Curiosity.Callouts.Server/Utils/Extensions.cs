using CitizenFX.Core;
using Curiosity.Callouts.Shared.EventWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Server.Utils
{
    public static class Extensions
    {
        public static void Trigger(this Event @event) => BaseScript.TriggerClientEvent(@event.Path);

        public static void Trigger<T>(this Event<T> @event, T field1) =>
            BaseScript.TriggerClientEvent(@event.Path, field1);

        public static void Trigger<T1, T2>(this Event<T1, T2> @event, T1 field1, T2 field2) =>
            BaseScript.TriggerClientEvent(@event.Path, field1, field2);

        public static void Trigger<T1, T2, T3>(this Player player, Event<T1, T2, T3> @event, T1 field1, T2 field2,
            T3 field3) =>
            player.TriggerEvent(@event.Path, field1, field2, field3);

        public static void Trigger<T1, T2, T3, T4>(this Player player, Event<T1, T2, T3, T4> @event, T1 field1,
            T2 field2, T3 field3, T4 field4) =>
            player.TriggerEvent(@event.Path, field1, field2, field3, field4);


        public static void Trigger<T1, T2, T3, T4, T5>(this Player player, Event<T1, T2, T3, T4, T5> @event, T1 field1,
            T2 field2, T3 field3, T4 field4, T5 field5) =>
            player.TriggerEvent(@event.Path, field1, field2, field3, field4, field5);


        public static void Trigger<T1, T2, T3, T4, T5, T6>(this Player player, Event<T1, T2, T3, T4, T5, T6> @event,
            T1 field1, T2 field2, T3 field3, T4 field4, T5 field5, T6 field6) =>
            player.TriggerEvent(@event.Path, field1, field2, field3, field4, field5, field6);


        public static void Trigger(this Player player, Event @event) =>
            player.TriggerEvent(@event.Path);

        public static void Trigger<T>(this Player player, Event<T> @event, T field) =>
            player.TriggerEvent(@event.Path, field);
    }
}
