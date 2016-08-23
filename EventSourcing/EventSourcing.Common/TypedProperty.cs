using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class TypedProperty
    {
        public TypedProperty(object value) { Value = value; }

        public object Value { get; set; }

        public string StringValue { get { return Value.ToString(); } }

        public Guid GuidValue { get { return (Guid)Value; } }

        public Int32 Int32Value { get { return (Int32)Value; } }

        public DateTimeOffset DateTimeOffsetValue { get { return DateTimeOffset.Parse(StringValue); } }
    }
}
