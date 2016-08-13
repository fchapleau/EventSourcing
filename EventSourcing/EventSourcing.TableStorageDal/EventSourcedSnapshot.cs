using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using System.Globalization;

namespace EventSourcing.Processors
{
    internal class EventSourcedSnapshot<T> : ITableEntity where T : IEntity, new()
    {
        public EventSourcedEvent SourceEvent { get; set; }
        public EventSourcedEntity<T> SourceSnapshot { get; set; }
        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public EventSourcedSnapshot()
        {
            SourceSnapshot = new EventSourcedEntity<T>();
            SourceEvent = new EventSourcedEvent();
        }

        public EventSourcedSnapshot(EventSourcedEntity<T> snapshot, EventSourcedEvent sourceEvent)
        {
            SourceSnapshot = snapshot;
            SourceEvent = sourceEvent;
            PartitionKey = SourceSnapshot.SourceEntity.UId.ToString();
            RowKey = sourceEvent.RowKey;
        }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            SourceSnapshot.ReadEntity(properties, operationContext);
            SourceEvent.ReadEntity(properties, operationContext);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var props1 = SourceSnapshot.WriteEntity(operationContext);
            var props2 = SourceEvent.WriteEntity(operationContext);
            foreach (var prop in props2)
                props1.Add(prop);
            return props1;
        }
    }
}
