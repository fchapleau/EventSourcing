using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using EventSourcing.Tests.Domain;

namespace EventSourcing.Tests
{
    [TestClass]
    public class ParallelTests : UnitTestBase
    {
        private Random _rand;
        [TestInitialize]
        public void TestInitialize()
        {
            _rand = new Random();
        }

        //[TestMethod]
        //public void ParallelSend500NoProcessing()
        //{
        //    Initialize(0, 0, 10, 10);
        //    ParallelSendEventsAndValidate(500);
        //}

        //[TestMethod]
        //public void ParallelSend1000NoProcessing()
        //{
        //    Initialize(0, 0, 10, 10);
        //    ParallelSendEventsAndValidate(1000);
        //}

        //[TestMethod]
        //public void ParallelSend5000NoProcessing()
        //{
        //    Initialize(0, 0, 10, 10);
        //    ParallelSendEventsAndValidate(5000);
        //}

        [TestMethod]
        public void ParallelSend50SequentialProcessing()
        {
            Initialize(1, 1, 10,10);
            ParallelSendEventsAndValidate(50);
        }

        [TestMethod]
        public void ParallelSend50ParallelProcessing10()
        {
            Initialize(10, 10, 10, 10);
            ParallelSendEventsAndValidate(50);
        }

        [TestMethod]
        public void ParallelSend150ParallelProcessing10()
        {
            Initialize(10, 10, 10, 10);
            ParallelSendEventsAndValidate(150);
        }

        [TestMethod]
        public void ParallelSend1000ParallelProcessing10()
        {
            Initialize(10, 10, 10, 10);
            ParallelSendEventsAndValidate(1000);
        }


        [TestMethod]
        public void ParallelSend5000ParallelProcessing10()
        {
            Initialize(10, 10, 10, 10);
            ParallelSendEventsAndValidate(5000);
        }

        [TestMethod]
        public void ParallelSend1000ParallelProcessing10NoSnapshot()
        {
            Initialize(10, 10, null, null);
            ParallelSendEventsAndValidate(1000);
        }

        [TestMethod]
        public void ParallelSend5000ParallelProcessing10NoSnapshot()
        {
            Initialize(10, 10, null, null);
            ParallelSendEventsAndValidate(5000);
        }

        public void ParallelSendEventsAndValidate(int numberOfTxs)
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test Symbol", Quantity = 1000 };
            var writeClient = Factory.CreateWriteSideClient();
            var readClient = Factory.CreateReadSideClient();
            writeClient.SendMessage(SymbolEvent.CreateInitialBuyRequest(symbol, null));

            WaitForClients(writeClient, readClient, 10000);

            ConcurrentBag<int> txs = new ConcurrentBag<int>();
            var parallelResult =  Parallel.For(0, numberOfTxs, a =>
            {
                txs.Add(SendRandomEvents(writeClient, symbol.UId));
            });

            Assert.IsTrue(parallelResult.IsCompleted);

            TestContext.WriteLine("****************************** Finished Sending Message ********************");


            WaitForClients(writeClient, readClient, 5000);

            Assert.AreEqual(numberOfTxs, txs.Count(), "Invalid number of transactions.");

            var client = Factory.CreateProjectionClient();
            var latest = client.GetEntity(symbol.UId);

            Assert.IsNotNull(latest, "Entity wasn't Projected.");
            Assert.AreEqual(symbol.Quantity +  txs.Sum(), latest.Quantity, "Result Incorrect");

            Assert.IsTrue(writeClient.MessageInErrorCount() == 0, "Messages in Error.");
        }

        public int SendRandomEvents(IMessagingClient writeSide, Guid symbolUid)
        {
            
            var quantity = _rand.Next(100);
            var tx = _rand.Next(2);

            if (tx == 1)
            {
                writeSide.SendMessage(SymbolEvent.CreateBuyRequest(symbolUid, quantity, null));
                return quantity;
            }
            else
            {
                writeSide.SendMessage(SymbolEvent.CreateSellRequest(symbolUid, quantity, null));
                return -quantity;
            }
        }
    }
}
