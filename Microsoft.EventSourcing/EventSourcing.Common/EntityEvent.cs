using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    [Serializable]
    public class EntityEvent : IComparable, IEquatable<EntityEvent> 
    {
        public EntityEvent()
        {
            Properties = new Dictionary<string, string>();
        }
        public Guid EventUId { get; set; }
        public DateTimeOffset EventTimeStamp { get; set; }
        public Guid EntityUId { get; set; }
        public string EventType { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public string OrderKey { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties)
        {
            foreach (var propertypair in properties)
            {
                if (propertypair.Key == "EventEventType") EventType = propertypair.Value.StringValue;
                else if (propertypair.Key == "EventEventUId") EventUId = propertypair.Value.GuidValue.Value;
                else if (propertypair.Key == "EventEntityUId") EntityUId = propertypair.Value.GuidValue.Value;
                else if (propertypair.Key == "EventEventTimestamp") EventTimeStamp = DateTimeOffset.Parse(propertypair.Value.StringValue);
                else if (propertypair.Key == "EventOrderKey") OrderKey = propertypair.Value.StringValue;
                else if (propertypair.Value.PropertyType == EdmType.String)
                    Properties.Add(propertypair.Key, propertypair.Value.StringValue);
            }
        }
        public IDictionary<string, EntityProperty> WriteEntity()
        {
            var output = new Dictionary<string, EntityProperty>();
            output.Add("EventEventType", new EntityProperty(EventType));
            output.Add("EventEventUId", new EntityProperty(EventUId));
            output.Add("EventEntityUId", new EntityProperty(EntityUId));
            output.Add("EventEventTimestamp", new EntityProperty(EventTimeStamp.ToString()));
            output.Add("EventOrderKey", new EntityProperty(OrderKey));

            foreach (var propertypair in Properties)
                output.Add(propertypair.Key, new EntityProperty(propertypair.Value));
            return output;
        }

        public override string ToString()
        {
            return string.Format("EventId : {0} | Timestamp: {1} | Event Type: {2} | Entity UId: {3} ", EventUId, EventTimeStamp.ToString(), EventType, EntityUId);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is EntityEvent)) throw new ArgumentOutOfRangeException("obj");
            var obj2 = obj as EntityEvent;
            return obj2.OrderKey.CompareTo(this.OrderKey);
        }

        public bool Equals(EntityEvent other)
        {
            return this.EventUId.Equals(other.EventUId) &&
                this.EventType.Equals(other.EventType) &&
                this.EntityUId.Equals(other.EntityUId) &&
                this.EventTimeStamp.Equals(other.EventTimeStamp) &&
                this.OrderKey.Equals(other.OrderKey);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityEvent)) return false;
            return this.Equals((EntityEvent)obj);
        }
    }
}
