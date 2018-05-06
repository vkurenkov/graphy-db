namespace GraphyDb
{
    public abstract class Entity
    {
        public EntityState State = EntityState.Unchanged;
        public DbEngine Db;

        public void Delete()
        {
            Db.Delete(this);
        }
    }
}