using System.Collections.Concurrent;

namespace GraphyDb
{
    public class EventualConsister
    {
        public static readonly BlockingCollection<Entity> ChangedEntitiesQueue = new BlockingCollection<Entity>();

    }
}