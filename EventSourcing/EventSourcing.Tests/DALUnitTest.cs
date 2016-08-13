using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using EventSourcing.InMemoryDal;
using EventSourcing.Tests.Domain;

namespace EventSourcing.Tests
{
    /// <summary>
    /// Summary description for DALUnitTest
    /// </summary>
    [TestClass]
    public class DALUnitTest
    {
        public DALUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        private int _orderKey;
        private string GetNextOrderKey()
        {
            _orderKey++;
            return (int.MaxValue - _orderKey).ToString() + DateTime.Now.Ticks.ToString();
        }

        [TestMethod]
        public void DALSimpleSnapshot()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test Symbol", Quantity = 100 };
            
            IDataAccessLayer<SymbolEntity> dal = new InMemoryDataAccessLayer<SymbolEntity>();

            dal.InsertEvent(SymbolEvent.CreateInitialBuyRequest(symbol, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));

            EntityEvent snapshotEvent = null;
            SymbolEntity snapshotEntity = default(SymbolEntity);
            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.IsNull(snapshotEvent);
            Assert.IsNull(snapshotEntity);

            var eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);

            Assert.IsTrue(Enumerable.Count(eventSourcedEvents) == 6);

            var instance = new SymbolAgregate();
            instance.Initialize(snapshotEntity, eventSourcedEvents);

            dal.InsertSnapshot(instance.Entity, instance.LatestEvent);

            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.IsNotNull(snapshotEvent);
            Assert.IsNotNull(snapshotEntity);

            eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);

            Assert.IsTrue(Enumerable.Count(eventSourcedEvents) == 0);

        }

        [TestMethod]
        public void DALSnapshotWithSkew()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test Symbol", Quantity = 100 };

            IDataAccessLayer<SymbolEntity> dal = new InMemoryDataAccessLayer<SymbolEntity>();

            dal.InsertEvent(SymbolEvent.CreateInitialBuyRequest(symbol, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));

            EntityEvent snapshotEvent = null;
            SymbolEntity snapshotEntity = default(SymbolEntity);
            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.IsNull(snapshotEvent);
            Assert.IsNull(snapshotEntity);

            var eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);

            Assert.IsTrue(eventSourcedEvents.Count() == 6);

            var aggregate = new SymbolAgregate();
            aggregate.Initialize(null, eventSourcedEvents);

            dal.InsertSnapshot(aggregate.Entity, aggregate.LatestEvent);

            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.IsNotNull(snapshotEvent);
            Assert.IsNotNull(snapshotEntity);

            eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);

            Assert.AreEqual(0, eventSourcedEvents.Count());

            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            var lastEventBeforeSkew = SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey());
            dal.InsertEvent(lastEventBeforeSkew);

            Thread.Sleep(4000);

            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            var lastEvent = SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey());
            dal.InsertEvent(lastEvent);

            eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);
            Assert.AreEqual(5, eventSourcedEvents.Count());

            aggregate.Initialize(snapshotEntity.Clone() as SymbolEntity, eventSourcedEvents);

            var eventsBeforeSkew = aggregate.GetEventsOlderthan(DateTime.Now.AddSeconds(-4));
            Assert.AreEqual(3, eventsBeforeSkew.Count());

            var snapshotInstance = new SymbolAgregate();
            snapshotInstance.Initialize(snapshotEntity.Clone() as SymbolEntity, eventsBeforeSkew);
            Assert.AreEqual(lastEventBeforeSkew, snapshotInstance.LatestEvent);

            dal.InsertSnapshot(snapshotInstance.Entity, snapshotInstance.LatestEvent);
            

            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);

            var eventsSinceLastSnapshot = dal.GetEventsSince(symbol.UId, snapshotEvent);
            Assert.AreEqual(2, eventsSinceLastSnapshot.Count());

            aggregate.Initialize(snapshotEntity, eventsSinceLastSnapshot);
            Assert.AreEqual(200, aggregate.Entity.Quantity);

        }


        [TestMethod]
        public void DALSnapshotsOrder()
        {
            EntityEvent snapshotEvent = null;
            EntityEvent lastEvent = null;
            SymbolEntity snapshotEntity = default(SymbolEntity);
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test Symbol", Quantity = 100 };

            IDataAccessLayer<SymbolEntity> dal = new InMemoryDataAccessLayer<SymbolEntity>();
            dal.InsertEvent(SymbolEvent.CreateInitialBuyRequest(symbol, GetNextOrderKey()));
            

            // First Snapshot
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            lastEvent = SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey());
            dal.InsertEvent(lastEvent);

            // Snapshot
            var eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);
            var aggregate = new SymbolAgregate();
            aggregate.Initialize(snapshotEntity, eventSourcedEvents);
            Assert.AreEqual(lastEvent, aggregate.LatestEvent);
            dal.InsertSnapshot(aggregate.Entity, aggregate.LatestEvent);

            // Validate snapshot
            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.AreEqual(aggregate.Entity, snapshotEntity);
            Assert.AreEqual(lastEvent, snapshotEvent);


            // Second Snapshot
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            dal.InsertEvent(SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey()));
            lastEvent = SymbolEvent.CreateBuyRequest(symbol.UId, 10, GetNextOrderKey());
            dal.InsertEvent(lastEvent);

            // Snapshot
            eventSourcedEvents = dal.GetEventsSince(symbol.UId, snapshotEvent);
            aggregate = new SymbolAgregate();
            aggregate.Initialize(snapshotEntity, eventSourcedEvents);
            Assert.AreEqual(lastEvent, aggregate.LatestEvent); 
            dal.InsertSnapshot(aggregate.Entity, aggregate.LatestEvent);

            // Validate snapshot
            dal.GetLatestSnapShot(symbol.UId, out snapshotEvent, out snapshotEntity);
            Assert.AreEqual(aggregate.Entity, snapshotEntity);
            Assert.AreEqual(lastEvent, snapshotEvent);

        }

    }
}
