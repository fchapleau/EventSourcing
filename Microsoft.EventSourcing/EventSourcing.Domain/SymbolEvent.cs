using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests.Domain
{
    public class SymbolEvent : EntityEvent
    {
        public static EntityEvent CreateInitialBuyRequest(SymbolEntity entity, string orderKey)
        {
            return new EntityEvent
            {
                EventUId = Guid.NewGuid(),
                EntityUId = entity.UId,
                EventTimeStamp = DateTime.Now,
                EventType = "InitialBuy",
                OrderKey = orderKey,
                Properties = new Dictionary<string, string> {
                    { "Quantity", entity.Quantity.ToString() },
                    { "Name", entity.Name },
                    { "UId", entity.UId.ToString() }
                }
            };
        }

        public static EntityEvent CreateSellRequest(Guid symbolUid, int quantity, string orderKey)
        {
            return new EntityEvent
            {
                EventUId = Guid.NewGuid(),
                EntityUId = symbolUid,
                EventTimeStamp = DateTime.Now,
                EventType = "Sell",
                OrderKey = orderKey,
                Properties = new Dictionary<string, string> {
                    { "Quantity", quantity.ToString() }
                }
            };
        }

        public static EntityEvent CreateBuyRequest(Guid symbolUid, int quantity, string orderKey = null)
        {
            return new EntityEvent
            {
                EventUId = Guid.NewGuid(),
                EntityUId = symbolUid,
                EventTimeStamp = DateTime.Now,
                EventType = "Buy",
                OrderKey = orderKey,
                Properties = new Dictionary<string, string> {
                    { "Quantity", quantity.ToString() }
                }
            };
        }
    }
}
