using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceBus.Messaging;
using System.Threading;
using Microsoft.EventSourcing.Processors;
using Microsoft.EventSourcing.Domain;
using System.Collections.Concurrent;

namespace Microsoft.EventSourcing.Tests
{
    [TestClass]
    public class ParallelTests : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        //[TestMethod]
        //public void ParallelSend500NoProcessing()
        //{
        //    Initialize(0, 0);
        //    ParallelSendEventsAndValidate(500);
        //}

        //[TestMethod]
        //public void ParallelSend1000NoProcessing()
        //{
        //    Initialize(0, 0);
        //    ParallelSendEventsAndValidate(1000);
        //}

        //[TestMethod]
        //public void ParallelSend5000NoProcessing()
        //{
        //    Initialize(0, 0);
        //    ParallelSendEventsAndValidate(5000);
        //}

        [TestMethod]
        public void ParallelSend50SequentialProcessing()
        {
            Initialize(1, 1);
            ParallelSendEventsAndValidate(50);
        }

        [TestMethod]
        public void ParallelSend50ParallelProcessing10()
        {
            Initialize(10, 10);
            ParallelSendEventsAndValidate(50);
        }

        [TestMethod]
        public void ParallelSend150ParallelProcessing10()
        {
            Initialize(10, 10);
            ParallelSendEventsAndValidate(150);
        }

        [TestMethod]
        public void ParallelSend1000ParallelProcessing10()
        {
            Initialize(10, 10);
            ParallelSendEventsAndValidate(1000);
        }

        [TestMethod]
        public void ParallelSend5000ParallelProcessing10()
        {
            Initialize(10, 10);
            ParallelSendEventsAndValidate(5000);
        }

        public void ParallelSendEventsAndValidate(int numberOfTxs)
        {
            var symbol = new SymbolEntity { UId = Guid.NewGuid(), Name = "Test Symbol", Quantity = 1000 };
            var writeClient = Factory.CreateWriteSideClient();
            var readClient = Factory.CreateReadSideClient();
            writeClient.SendMessage(SymbolEvent.CreateInitialBuyRequest(symbol, null));

            WaitForClients(writeClient, readClient);

            ConcurrentBag<int> txs = new ConcurrentBag<int>();
            Parallel.For(0, numberOfTxs, a =>
            {
                txs.Add(SendRandomEvents(writeClient, symbol.UId));
                //writeClient.SendMessage(SymbolEvent.CreateBuyRequest(symbol.UId, 1, null));
                //txs.Add(1);
                //Thread.Sleep(5);
            });

            TestContext.WriteLine("****************************** Finished Sending Message ********************");


            WaitForClients(writeClient, readClient);

            Assert.AreEqual(numberOfTxs, txs.Count(), "Invalid number of transactions.");

            var client = Factory.CreateProjectionClient();
            var latest = client.GetEntity(symbol.UId);

            Assert.IsNotNull(latest, "Entity wasn't Projected.");
            Assert.AreEqual(symbol.Quantity +  txs.Sum(), latest.Quantity, "Result Incorrect");

            Assert.IsTrue(writeClient.MessageInErrorCount() == 0, "Messages in Error.");
        }

        public static int SendRandomEvents(IMessagingClient writeSide, Guid symbolUid)
        {
            var rand = new Random();
            var quantity = rand.Next(100);
            var tx = rand.Next(2);

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
