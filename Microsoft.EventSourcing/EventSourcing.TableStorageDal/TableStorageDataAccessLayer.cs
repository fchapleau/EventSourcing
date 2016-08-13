using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Processors
{
    public class TableStorageDataAccessLayer<T> : IDataAccessLayer<T> where T : IEntity, new()
    {
        private CloudStorageAccount _account;

        public TableStorageDataAccessLayer(string accountName, string keyValue)
        {
            var storageCredentials = new StorageCredentials(accountName, keyValue);
            _account = new CloudStorageAccount(storageCredentials, true);
        }

        public IOrderedEnumerable<EntityEvent> GetEventsSince(Guid entityId, EntityEvent entityEvent)
        {
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, entityId.ToString());

            if (entityEvent != null)
                condition = TableQuery.CombineFilters(condition, TableOperators.And, TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, entityEvent.OrderKey));
            
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name + "Events");
            var results = table.ExecuteQuery<EventSourcedEvent>(new TableQuery<EventSourcedEvent>().Where(condition));

            return results.Select(s=>s.EntityEvent).OrderByDescending(t=>t.OrderKey);
        }

        public void InsertEvent(EntityEvent entityEvent)
        {
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name +"Events");
            var operation = TableOperation.Insert(new EventSourcedEvent(entityEvent));
            table.Execute(operation);
        }

        public T GetEntity(Guid uId)
        {
            var query = new TableQuery<EventSourcedEntity<T>>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, uId.ToString()));

            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name);
            var results = table.ExecuteQuery<EventSourcedEntity<T>>(query);
            var result = results.FirstOrDefault();

            if (result == null)
                return default(T);
            else
                return (T)result.SourceEntity;
        }

        public void GetLatestSnapShot(Guid UId, out EntityEvent snapshotEvent, out T snapshotEntity)
        {
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, UId.ToString());
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name + "Snapshots");
            var results = table.ExecuteQuery<EventSourcedSnapshot<T>>(new TableQuery<EventSourcedSnapshot<T>>().Where(condition));
            var result = results.LastOrDefault();

            if (result != null)
            {
                snapshotEvent = result.SourceEvent.EntityEvent;
                snapshotEntity = result.SourceSnapshot.SourceEntity;
            }
            else
            {
                snapshotEvent = null;
                snapshotEntity = default(T);
            }
        }

        public void InsertEntity(T entity)
        {
            var eventSourcedEntity = new EventSourcedEntity<T>(entity);
            var operation = TableOperation.InsertOrReplace(eventSourcedEntity);
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name);
            table.Execute(operation);
        }

        public void InsertSnapshot(T entity, EntityEvent e)
        {
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name + "Snapshots");
            var eventSourcedEntity = new EventSourcedSnapshot<T>(new EventSourcedEntity<T>(entity), new EventSourcedEvent(e));
            var operation = TableOperation.InsertOrReplace(eventSourcedEntity);
            table.Execute(operation);
        }
        
        public void DeleteSnapshots(Guid entityUId)
        {
            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference(typeof(T).Name + "Snapshots");

            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, entityUId.ToString());

            var results = table.ExecuteQuery<EventSourcedSnapshot<T>>(new TableQuery<EventSourcedSnapshot<T>>().Where(condition));
            foreach (var result in results)
            {
                var deleteOperation = TableOperation.Delete(result);

                try
                {
                    table.Execute(deleteOperation);
                }
                catch (StorageException wex)
                {
                    if (wex.RequestInformation.HttpStatusCode != 404)
                        throw;
                }
            }
        }
    }
}
