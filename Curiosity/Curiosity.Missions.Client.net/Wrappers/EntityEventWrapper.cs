using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Curiosity.Missions.Client.net.Wrappers
{
    class EntityEventWrapper
    {
        private static Client client = Client.GetInstance();

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
                    else
                    {
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

            client.RegisterTickHandler(this.OnTick);
            //ScriptEventHandler.Instance.RegisterWrapper(new EventHandler(this.OnTick));
            //ScriptEventHandler.Instance.add_Aborted((object sender, EventArgs args) => this.Abort());
            EntityEventWrapper.Wrappers.Add(this);
        }

        public void Abort()
        {
            EntityEventWrapper.OnWrapperAbortedEvent onWrapperAbortedEvent = this.Aborted;
            if (onWrapperAbortedEvent != null)
            {
                onWrapperAbortedEvent(this, this.Entity);
            }
            else
            {
            }
        }

        public void Dispose()
        {
            client.DeregisterTickHandler(this.OnTick);

            EntityEventWrapper.Wrappers.Remove(this);
            EntityEventWrapper.OnWrapperDisposedEvent onWrapperDisposedEvent = this.Disposed;
            if (onWrapperDisposedEvent != null)
            {
                onWrapperDisposedEvent(this, this.Entity);
            }
            else
            {
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

        public async Task OnTick()
        {
            if ((this.Entity == null ? false : this.Entity.Exists()))
            {
                this.IsDead = this.Entity.IsDead;
                EntityEventWrapper.OnWrapperUpdateEvent onWrapperUpdateEvent = this.Updated;
                if (onWrapperUpdateEvent != null)
                {
                    onWrapperUpdateEvent(this, this.Entity);
                }
                else
                {
                }
            }
            else
            {
                this.Dispose();
            }
            await Task.FromResult(0);
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
