using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using System.Globalization;

namespace Microsoft.EventSourcing.Processors
{
    internal class EventSourcedEvent : ITableEntity
    {
        public EntityEvent EntityEvent { get; set; }

        public EventSourcedEvent()
        {            
        }

        public EventSourcedEvent(EntityEvent e)
        {
            EntityEvent = e;
            PartitionKey = e.EntityUId.ToString();
            RowKey = e.OrderKey;       
        }

        public string ETag { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            EntityEvent = new EntityEvent();
            var prefix = "Event";
            var toProcess = new Dictionary<string, EntityProperty>();
            foreach (var prop in properties)
                if(prop.Key.StartsWith(prefix))
                    toProcess.Add(prop.Key.Substring(prefix.Length), prop.Value);
            EntityEvent.ReadEntity(toProcess);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var prefix = "Event";
            var toReturn = new Dictionary<string, EntityProperty>();
            var props = EntityEvent.WriteEntity();
            foreach (var prop in props)
                toReturn.Add(prefix + prop.Key, prop.Value);
            return toReturn;
        }
    }
}
