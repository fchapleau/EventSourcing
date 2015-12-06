using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing
{
    public interface IEntityAggregate<T> where T :IEntity
    {
        void Initialize(T snapshot, IOrderedEnumerable<EntityEvent> events);
        T Entity { get; }
        EntityEvent LatestEvent { get; }
        IOrderedEnumerable<EntityEvent> GetEventsOlderthan(DateTime span);
    }
}
