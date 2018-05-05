namespace GraphyDb
{
    public abstract class Entity
    {
        public EntityState State = EntityState.Unchanged;
        public UnitOfWork Db;
    }
}