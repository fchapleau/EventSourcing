using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceBus.Messaging;
using System.Threading;
using EventSourcing.Tests.Domain;

namespace EventSourcing.Tests
{
    [TestClass]
    public class SimpleTest : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(10, 10, 10,10);
        }

        [TestMethod]
        public void SingleAndMaterialize()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "MSFT", Quantity = 50000 };

            var writeClient = Factory.CreateWriteSideClient();
            var readClient = Factory.CreateReadSideClient();
            writeClient.SendMessage(SymbolEvent.CreateInitialBuyRequest(symbol, (int.MaxValue - 1).ToString()));

            var client = Factory.CreateProjectionClient();

            WaitForClients(writeClient, readClient, 5000);

            var entity = client.GetEntity(symbol.UId);
            Assert.IsNotNull(entity);
            Assert.AreEqual(entity.Name, symbol.Name);
            Assert.AreEqual(entity.Quantity, symbol.Quantity);
            Assert.IsTrue(entity.Equals(symbol));
        }

        [TestMethod]
        public void MultipleAndMaterialize()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Symbol2", Quantity = 50000 };

            var readClient = Factory.CreateReadSideClient();
            var writeClient = Factory.CreateWriteSideClient();
            writeClient.SendMessage(SymbolEvent.CreateInitialBuyRequest(symbol, (int.MaxValue - 1).ToString()));

            var client = Factory.CreateProjectionClient();

            WaitForClients(writeClient, readClient, 5000);

            var entity = client.GetEntity(symbol.UId);
            Assert.AreEqual(entity.Name, symbol.Name);
            Assert.AreEqual(entity.Quantity, symbol.Quantity);
            
            writeClient.SendMessage(SymbolEvent.CreateBuyRequest(symbol.UId, 100));

            WaitForClients(writeClient, readClient, 5000);

            entity = client.GetEntity(symbol.UId);
            Assert.AreEqual(symbol.Quantity + 100, entity.Quantity );

            writeClient.SendMessage(SymbolEvent.CreateSellRequest( symbol.UId, 100, (int.MaxValue - 1).ToString()));
            writeClient.SendMessage(SymbolEvent.CreateBuyRequest(symbol.UId, 100, (int.MaxValue - 2).ToString()));
            writeClient.SendMessage(SymbolEvent.CreateSellRequest(symbol.UId, 100, (int.MaxValue - 3).ToString()));
            writeClient.SendMessage(SymbolEvent.CreateSellRequest(symbol.UId, 100, (int.MaxValue - 4).ToString()));
            writeClient.SendMessage(SymbolEvent.CreateBuyRequest(symbol.UId, 100, (int.MaxValue - 5).ToString()));

            WaitForClients(writeClient, readClient, 5000);

            entity = client.GetEntity(symbol.UId);
            Assert.AreEqual(symbol.Quantity, entity.Quantity);

            
        }
    }
}
