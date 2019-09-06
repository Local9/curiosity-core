using System;
using System.Collections;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Helpers
{
    public abstract class EntitiesPool<T> : IEnumerable<T>, IEnumerable
    {
        private Hash FindFirst { get; set; }
        private Hash FindNext { get; set; }
        private Hash EndFind { get; set; }

        public EntitiesPool(uint findFirst, uint findNext, uint endFind)
        {
            FindFirst = (Hash)findFirst;
            FindNext = (Hash)findNext;
            EndFind = (Hash)endFind;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            var foundEntity = new OutputArgument();

            int handle = Function.Call<int>(FindFirst, foundEntity);

            if (handle == -1)
            {
                yield break;
            }

            int entityHandle;
            var hasMore = true;

            while (hasMore)
            {
                entityHandle = foundEntity.GetResult<int>();

                if (entityHandle == -1)
                {
                    continue;
                }

                if (CastSilently(entityHandle, out T entity))
                {
                    yield return entity;
                }

                hasMore = Function.Call<bool>(FindNext, handle, foundEntity);
            }

            Function.Call(EndFind, handle);
        }

        protected bool CastSilently(int handle, out T entity)
        {
            try
            {
                entity = Cast(handle);
                return true;
            }
            catch (Exception e)
            {
                entity = default(T);
                return false;
            }
        }

        protected abstract T Cast(int hanlde);
    }

    public class PedsPool : EntitiesPool<Ped>
    {
        public PedsPool() : base(0xfb012961, 0xab09b548, 0x9615c2ad) { }
        protected override Ped Cast(int handle) => new Ped(handle);
    }

    public class ObjectsPool : EntitiesPool<Prop>
    {
        public ObjectsPool() : base(0xfaa6cb5d, 0x4e129dbf, 0xdeda4e50) { }
        protected override Prop Cast(int handle) => new Prop(handle);
    }

    public class VehiclesPool : EntitiesPool<Vehicle>
    {
        public VehiclesPool() : base(0x15e55694, 0x8839120d, 0x9227415a) { }
        protected override Vehicle Cast(int handle) => new Vehicle(handle);
    }

    public class PickupsPool : EntitiesPool<Pickup>
    {
        public PickupsPool() : base(0x3ff9d340, 0x4107ef0f, 0x3c407d53) { }
        protected override Pickup Cast(int handle) => new Pickup(handle);
    }
}
