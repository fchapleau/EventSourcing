using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.InMemoryDal
{
    public class InMemoryDataAccessLayer<T> : IDataAccessLayer<T> where T : IEntity, new()
    {
        private ConcurrentDictionary<Guid, T> _entities;
        private ConcurrentDictionary<Guid, ConcurrentBag<EntityEvent>> _events;
        private ConcurrentDictionary<Guid, ConcurrentBag<Tuple<EntityEvent, T>>> _snapshots;        

        public InMemoryDataAccessLayer()
        {
            _entities = new ConcurrentDictionary<Guid, T>();
            _events = new ConcurrentDictionary<Guid, ConcurrentBag<EntityEvent>>();
            _snapshots = new ConcurrentDictionary<Guid, ConcurrentBag<Tuple<EntityEvent, T>>>();
        }

        public void DeleteSnapshots(Guid entityUId)
        {
            throw new NotImplementedException();
        }

        public T GetEntity(Guid uId)
        {
            if (_entities.ContainsKey(uId))
                return _entities[uId];
            else
                return default(T);
        }

        public IOrderedEnumerable<EntityEvent> GetEventsSince(Guid entityId, EntityEvent entityEvent)
        {
            if (entityEvent == null)
                return _events[entityId].OrderByDescending(t => t.OrderKey);
            else
                return _events[entityId].Where(f => f.OrderKey.CompareTo(entityEvent.OrderKey) < 0).OrderByDescending(f => f.OrderKey);
        }

        public void GetLatestSnapShot(Guid UId, out EntityEvent snapshotEvent, out T snapshotEntity)
        {
            if (_snapshots.ContainsKey(UId))
            {
                var latestTuple = _snapshots[UId].OrderByDescending(f => f.Item1.OrderKey).LastOrDefault();
                snapshotEvent = latestTuple.Item1;
                snapshotEntity = latestTuple.Item2;
            }
            else
            {
                snapshotEvent = null;
                snapshotEntity = default(T);
            }
        }

        public void InsertEntity(T entity)
        {
            _entities.AddOrUpdate(entity.UId, entity, (key, value) => entity);
        }

        public void InsertEvent(EntityEvent entityEvent)
        {
            if (string.IsNullOrEmpty(entityEvent.OrderKey))
                throw new ArgumentNullException("OrderKey can't be null.");

            Monitor.Enter(_events);
            if (!_events.ContainsKey(entityEvent.EntityUId))
                _events.AddOrUpdate(entityEvent.EntityUId, new ConcurrentBag<EntityEvent>(), (key, value) => new ConcurrentBag<EntityEvent>());
            Monitor.Exit(_events);

            _events[entityEvent.EntityUId].Add(entityEvent);
        }

        public void InsertSnapshot(T entity, EntityEvent e)
        {
            Monitor.Enter(_snapshots);
            if (!_snapshots.ContainsKey(entity.UId))
                _snapshots.AddOrUpdate(entity.UId, new ConcurrentBag<Tuple<EntityEvent, T>>(), (key, value) => new ConcurrentBag<Tuple<EntityEvent, T>>());
            Monitor.Exit(_snapshots);

            _snapshots[entity.UId].Add(new Tuple<EntityEvent, T>(e, entity));

        }
    }
}
