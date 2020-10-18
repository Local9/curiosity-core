using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Curiosity.Missions.Client.Wrappers
{
    class EntityEventWrapper
    {
        private static PluginManager PluginInstance => PluginManager.Instance;

        private readonly static List<EntityEventWrapper> Wrappers;

        private bool _isDead;

        public Entity Entity
        {
            get;
        }

        public bool IsDead
        {
            get
            {
                return this.Entity.IsDead;
            }
            private set
            {
                if ((!value ? false : !this._isDead))
                {
                    EntityEventWrapper.OnDeathEvent onDeathEvent = this.Died;
                    if (onDeathEvent != null)
                    {
                        onDeathEvent(this, this.Entity);
                    }
                }
                this._isDead = value;
            }
        }

        static EntityEventWrapper()
        {
            EntityEventWrapper.Wrappers = new List<EntityEventWrapper>();
        }

        public EntityEventWrapper(Entity ent)
        {
            this.Entity = ent;

            PluginInstance.RegisterTickHandler(this.EntityEventWrapperOnTick);
            EntityEventWrapper.Wrappers.Add(this);
        }

        public void Abort()
        {
            EntityEventWrapper.OnWrapperAbortedEvent onWrapperAbortedEvent = this.Aborted;
            if (onWrapperAbortedEvent != null)
            {
                onWrapperAbortedEvent(this, this.Entity);
            }
        }

        public void Dispose()
        {
            PluginInstance.DeregisterTickHandler(this.EntityEventWrapperOnTick);

            EntityEventWrapper.Wrappers.Remove(this);
            EntityEventWrapper.OnWrapperDisposedEvent onWrapperDisposedEvent = this.Disposed;
            if (onWrapperDisposedEvent != null)
            {
                onWrapperDisposedEvent(this, this.Entity);
            }
        }

        public static void Dispose(Entity entity)
        {
            EntityEventWrapper entityEventWrapper = EntityEventWrapper.Wrappers.Find((EntityEventWrapper w) => w.Entity == entity);
            if (entityEventWrapper != null)
            {
                entityEventWrapper.Dispose();
            }
            EntityEventWrapper.Wrappers.Remove(entityEventWrapper);
        }

        public async Task EntityEventWrapperOnTick()
        {
            if ((this.Entity == null ? false : this.Entity.Exists()))
            {
                this.IsDead = this.Entity.IsDead;
                EntityEventWrapper.OnWrapperUpdateEvent onWrapperUpdateEvent = this.Updated;
                if (onWrapperUpdateEvent != null)
                {
                    onWrapperUpdateEvent(this, this.Entity);
                }
            }
            else
            {
                this.Dispose();
            }
            await PluginManager.Delay(500);
        }

        public event EntityEventWrapper.OnWrapperAbortedEvent Aborted;

        public event EntityEventWrapper.OnDeathEvent Died;

        public event EntityEventWrapper.OnWrapperDisposedEvent Disposed;

        public event EntityEventWrapper.OnWrapperUpdateEvent Updated;

        public delegate void OnDeathEvent(EntityEventWrapper sender, Entity entity);

        public delegate void OnWrapperAbortedEvent(EntityEventWrapper sender, Entity entity);

        public delegate void OnWrapperDisposedEvent(EntityEventWrapper sender, Entity entity);

        public delegate void OnWrapperUpdateEvent(EntityEventWrapper sender, Entity entity);
    }
}
