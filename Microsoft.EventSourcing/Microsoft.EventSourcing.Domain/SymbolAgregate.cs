using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.Domain
{
    public class SymbolAgregate : IEntityAggregate<SymbolEntity>
    {
        private IOrderedEnumerable<EntityEvent> _events;
        public SymbolEntity Entity { get; private set; }

        public void Initialize(SymbolEntity symbol, IOrderedEnumerable<EntityEvent> events)
        {
            _events = events;
            Entity = ProcessEvents(symbol, events);
        }

        static private SymbolEntity ProcessEvents(SymbolEntity symbol, IOrderedEnumerable<EntityEvent> events)
        {            
            foreach (var e in events)
            {
                if (e.EventType == "InitialBuy")
                {
                    symbol = new SymbolEntity();
                    symbol.UId = e.EntityUId;
                    if (e.Properties.ContainsKey("Name"))
                        symbol.Name = e.Properties["Name"];
                    if (e.Properties.ContainsKey("Quantity"))
                        symbol.Quantity = int.Parse(e.Properties["Quantity"]);
                }
                if(e.EventType == "Buy" && symbol != null)
                {
                    if (e.Properties.ContainsKey("Quantity"))
                        symbol.Quantity += int.Parse(e.Properties["Quantity"]);
                }

                if (e.EventType == "Sell" && symbol != null)
                {
                    if (e.Properties.ContainsKey("Quantity"))
                        symbol.Quantity -= int.Parse(e.Properties["Quantity"]);
                }
            }
            return symbol;
        }


        public EntityEvent LatestEvent
        {
            get
            {
                return _events.OrderByDescending(t => t.OrderKey).LastOrDefault();
            }
        }

        public IOrderedEnumerable<EntityEvent> GetEventsOlderthan(DateTime span)
        {
            return _events.Where(t => t.EventTimeStamp < span).OrderByDescending(t=>t.OrderKey);
        }
    }
}
