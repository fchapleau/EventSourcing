using Microsoft.EventSourcing.Domain;
using Microsoft.EventSourcing.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.Tests
{
    [TestClass]
    public class TableStorageTest : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(10, 10);
        }
        
        [TestMethod]
        public void InsertAndSelect()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Symbol2", Quantity = 50000 };

            Dal.InsertEvent(SymbolEvent.CreateInitialBuyRequest(symbol, (DateTimeOffset.MaxValue.Ticks - 1).ToString()));
            Dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 100, (DateTimeOffset.MaxValue.Ticks - 2).ToString()));
            Dal.InsertEvent(SymbolEvent.CreateSellRequest(symbol.UId, 100, (DateTimeOffset.MaxValue.Ticks - 3).ToString()));

            var lastEvent = SymbolEvent.CreateSellRequest(symbol.UId, 100, (DateTimeOffset.MaxValue.Ticks - 4).ToString());
            Dal.InsertEvent(lastEvent);

            var events = Dal.GetEventsSince(symbol.UId, null);

            var retreivedLastEvent = events.OrderBy(t => t.OrderKey).FirstOrDefault();

            Assert.AreEqual(retreivedLastEvent.EventUId, lastEvent.EventUId);
        }
    }
}
