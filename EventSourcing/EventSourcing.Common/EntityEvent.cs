using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    [Serializable]
    public class EntityEvent : IComparable, IEquatable<EntityEvent>, ICloneable
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

        public void ReadEntity(IDictionary<string, TypedProperty> properties)
        {
            foreach (var propertypair in properties)
            {
                if (propertypair.Key == "EventEventType") EventType = propertypair.Value.StringValue;
                else if (propertypair.Key == "EventEventUId") EventUId = propertypair.Value.GuidValue;
                else if (propertypair.Key == "EventEntityUId") EntityUId = propertypair.Value.GuidValue;
                else if (propertypair.Key == "EventEventTimestamp") EventTimeStamp = propertypair.Value.DateTimeOffsetValue;
                else if (propertypair.Key == "EventOrderKey") OrderKey = propertypair.Value.StringValue;
                else 
                    Properties.Add(propertypair.Key, propertypair.Value.ToString());
            }
        }
        public IDictionary<string, TypedProperty> WriteEntity()
        {
            var output = new Dictionary<string, TypedProperty>();
            output.Add("EventEventType", new TypedProperty(EventType));
            output.Add("EventEventUId", new TypedProperty(EventUId));
            output.Add("EventEntityUId", new TypedProperty(EntityUId));
            output.Add("EventEventTimestamp", new TypedProperty(EventTimeStamp.ToString()));
            output.Add("EventOrderKey", new TypedProperty(OrderKey));

            foreach (var propertypair in Properties)
                output.Add(propertypair.Key, new TypedProperty(propertypair.Value));
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

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
