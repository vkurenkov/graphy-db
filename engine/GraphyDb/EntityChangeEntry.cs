namespace GraphyDb
{

    public enum EntityState
    {
        Unchanged = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3
    }


    public class EntityChangeEntry
    {
        public Entity Entity;

        public EntityState State;

    }
}