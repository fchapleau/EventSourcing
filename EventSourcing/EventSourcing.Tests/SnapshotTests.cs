using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventSourcing.Tests.Domain;

namespace EventSourcing.Tests
{
    [TestClass]
    public class SnapshotTests: UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(10, 10, 10,10);
        }

        [TestMethod]
        public void SnapshotNullTest()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test", Quantity = 50000 };
            var symbolEvents = new List<EntityEvent>();
            symbolEvents.Add(SymbolEvent.CreateInitialBuyRequest(symbol, (int.MaxValue -1).ToString()));
            symbolEvents.ForEach(e => Dal.InsertEvent(e));
            
            var agregate = new SymbolAgregate();
            agregate.Initialize(null, Dal.GetEventsSince(symbol.UId, null));
            Dal.InsertEntity(agregate.Entity);

            var retreivedEntity = Dal.GetEntity(symbol.UId);

            Assert.AreEqual<SymbolEntity>(agregate.Entity, retreivedEntity);
        }

        [TestMethod]
        public void SnapshotSingleTest()
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test", Quantity = 50000 };
            
            var symbolEvents = new List<EntityEvent>();
            symbolEvents.Add(SymbolEvent.CreateInitialBuyRequest(symbol, (int.MaxValue - 1).ToString()));
            symbolEvents.Add(SymbolEvent.CreateSellRequest(symbol.UId, 100, (int.MaxValue - 2).ToString())); 
            symbolEvents.ForEach(e => Dal.InsertEvent(e));

            var agregate = new SymbolAgregate();
            agregate.Initialize(null, Dal.GetEventsSince(symbol.UId, null));
            Dal.InsertEntity(agregate.Entity);

            var retreivedEntity = Dal.GetEntity(symbol.UId);

            Assert.AreEqual<SymbolEntity>(agregate.Entity, retreivedEntity);

            Dal.InsertSnapshot(agregate.Entity, symbolEvents[1]);

            EntityEvent returnedEvent = null;
            SymbolEntity returnedSnapshot = null;
            Dal.GetLatestSnapShot(symbol.UId, out returnedEvent, out returnedSnapshot);

            Assert.IsNotNull(returnedSnapshot);
            Assert.AreEqual(returnedSnapshot, agregate.Entity);

            
            
        }


        [TestMethod]
        public void SnapshotDoubleTest()
        {
            EntityEvent returnedEvent = null;
            SymbolEntity returnedSnapshot = null;
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test", Quantity = 50000 };
            
            // Initialize
            var symbolEvents = new List<EntityEvent>();
            symbolEvents.Add(SymbolEvent.CreateInitialBuyRequest(symbol, (int.MaxValue -1).ToString()));
            symbolEvents.Add(SymbolEvent.CreateSellRequest(symbol.UId, 100, (int.MaxValue - 2).ToString()));
            symbolEvents.ForEach(e => Dal.InsertEvent(e));

            // Get Events, Create Agregate and Perist Materialized View
            var aggregate = new SymbolAgregate();
            aggregate.Initialize(null, Dal.GetEventsSince(symbol.UId, null));
            Dal.InsertEntity(aggregate.Entity);
            Assert.AreEqual<SymbolEntity>(aggregate.Entity, Dal.GetEntity(symbol.UId));
            
            // Insert the latest entity as a new snapshot, related to the last event
            Dal.InsertSnapshot(aggregate.Entity, aggregate.LatestEvent);
            Dal.GetLatestSnapShot(symbol.UId, out returnedEvent, out returnedSnapshot);
            Assert.AreEqual(returnedSnapshot, aggregate.Entity);
            Assert.AreEqual(returnedEvent, aggregate.LatestEvent);

            // Create two new events
            symbolEvents.Clear();
            symbolEvents.Add(SymbolEvent.CreateSellRequest(symbol.UId, 101, (int.MaxValue - 3).ToString()));
            symbolEvents.Add(SymbolEvent.CreateSellRequest(symbol.UId, 102, (int.MaxValue - 4).ToString()));
            symbolEvents.Add(SymbolEvent.CreateSellRequest(symbol.UId, 102, (int.MaxValue - 5).ToString()));
            symbolEvents.ForEach(e => Dal.InsertEvent(e));
            
            // Create a new agregate based on the latest Snapshot and based on the events not processed since then
            Dal.GetLatestSnapShot(symbol.UId, out returnedEvent, out returnedSnapshot);
            Assert.AreEqual(returnedSnapshot, aggregate.Entity);
            //Assert.AreEqual(returnedEvent, aggregate.LatestEvent);

            var eventsNotProcessed = Dal.GetEventsSince(symbol.UId, returnedEvent);
            Assert.AreEqual(3, eventsNotProcessed.Count());

            aggregate = new SymbolAgregate();
            aggregate.Initialize(returnedSnapshot, eventsNotProcessed);
            Dal.InsertSnapshot(aggregate.Entity, aggregate.LatestEvent);
            Dal.GetLatestSnapShot(symbol.UId, out returnedEvent, out returnedSnapshot);
            Assert.AreEqual(returnedSnapshot, aggregate.Entity);
            Assert.AreEqual(returnedEvent, aggregate.LatestEvent);


            eventsNotProcessed = Dal.GetEventsSince(symbol.UId, returnedEvent);

            Assert.AreEqual(0, eventsNotProcessed.Count());            
        }

        /*
        public void stub()
        {


            var lastestSnapshotTime = DateTimeOffset.MinValue;
            T snapshotEntity = default(T);
            if (eventSourcedSnapShot != null)
            {
                lastestSnapshotTime = eventSourcedSnapShot.SourceEvent.EntityEvent.EventTimeStamp;
                snapshotEntity = eventSourcedSnapShot.SourceSnapshot.SourceEntity;
            }

            if (eventReceived.EventTimeStamp > lastestSnapshotTime)
            {
                var eventSourcedEvents = _dal.GetEvents(eventReceived.EntityUId, lastestSnapshotTime);
                var entityEvents = eventSourcedEvents.Select(f => f.EntityEvent).ToList();
                _logger.WriteLine(LoggingLevel.Information, string.Format("Read Side [{0}], Processing entity {1} with latest Snapshot at {2} and {3} events.", i, eventReceived.EntityUId, lastestSnapshotTime, entityEvents.Count()));

                agregate.Initialize(snapshotEntity, entityEvents);

                if (agregate.Entity != null)
                {
                    var eventSourcedEvent = _dal.InsertEntity(instance.Entity);
                    eventSourcedEvent.Timestamp = eventReceived.EventTimeStamp;
                    if (entityEvents.Count > 10)
                    {
                        _logger.WriteLine(LoggingLevel.Warning, string.Format("Read Side [{0}], *** Creating New Snapshot for {1} events at {2}.", i.ToString(), entityEvents.Count(), eventReceived.EventTimeStamp.ToString()));
                        _dal.InsertSnapshot(new EventSourcedSnapshot<T>(eventSourcedEvent, new EventSourcedEvent(eventReceived)));
                    }
                }
                else
                {

                }
            }
            else
            {
                _logger.WriteLine(LoggingLevel.Warning, "Read Side [" + i.ToString() + "], Discarding future event contained in last snapshot.");
            }
        }*/

    }
}
