using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace EventSourcing.Processors
{
    internal class EventSourcedEntity<T> : ITableEntity where T : IEntity, new()
    {
        public T SourceEntity { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public EventSourcedEntity()
        {
            SourceEntity = new T();
        }

        public EventSourcedEntity(T e)
        {
            SourceEntity = e;
            PartitionKey = SourceEntity.GetType().Name;
            RowKey = SourceEntity.UId.ToString();
        }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var prefix = typeof(T).Name + "";
            var toProcess = new Dictionary<string, EntityProperty>();
            foreach (var prop in properties)
                if (prop.Key.StartsWith(prefix))
                    toProcess.Add(prop.Key.Substring(prefix.Length), prop.Value);
            SourceEntity.ReadEntity(toProcess);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var toReturn = new Dictionary<string, EntityProperty>();
            var props = SourceEntity.WriteEntity();
            foreach (var prop in props)
                toReturn.Add(typeof(T).Name + "" + prop.Key, prop.Value);
            return toReturn;
        }
    }
}
