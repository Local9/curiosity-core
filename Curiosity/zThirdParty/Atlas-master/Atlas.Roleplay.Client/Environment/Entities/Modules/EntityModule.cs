namespace Atlas.Roleplay.Client.Environment.Entities.Modules
{
    public abstract class EntityModule
    {
        public AtlasEntity Entity { get; set; }
        public int Id { get; set; }

        public void CallBeginOperation(AtlasEntity entity, int id)
        {
            Entity = entity;
            Id = id;

            Begin(entity, id);
        }

        public T As<T>() where T : EntityModule
        {
            return (T)this;
        }

        protected abstract void Begin(AtlasEntity entity, int id);
    }
}