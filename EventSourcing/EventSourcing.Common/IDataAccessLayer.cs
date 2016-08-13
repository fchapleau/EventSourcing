using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public interface IDataAccessLayer<T>
     where T : IEntity, new()
    {
        void DeleteSnapshots(Guid entityUId);
        T GetEntity(Guid uId);
        IOrderedEnumerable<EntityEvent> GetEventsSince(Guid entityId, EntityEvent entityEvent);
        void GetLatestSnapShot(Guid UId, out EntityEvent snapshotEvent, out T snapshotEntity);
        void InsertEntity(T entity);
        void InsertEvent(EntityEvent entityEvent);
        void InsertSnapshot(T entity, EntityEvent e);
    }
}
