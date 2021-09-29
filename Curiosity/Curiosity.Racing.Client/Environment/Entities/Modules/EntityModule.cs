
namespace Curiosity.Racing.Client.Environment.Entities.Modules
{
    public abstract class EntityModule
    {
        public CuriosityEntity Entity { get; set; }
        public int Id { get; set; }

        public void CallBeginOperation(CuriosityEntity entity, int id)
        {
            Entity = entity;
            Id = id;

            Begin(entity, id);
        }

        public T As<T>() where T : EntityModule
        {
            return (T)this;
        }

        protected abstract void Begin(CuriosityEntity entity, int id);
    }
}