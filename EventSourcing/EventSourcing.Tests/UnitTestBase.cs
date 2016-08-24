﻿using System;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using EventSourcing.Processors;
using EventSourcing.Tests.Domain;
using System.Collections.Generic;
using EventSourcing.InMemoryDal;

namespace EventSourcing.Tests
{
    [TestClass]
    public class UnitTestBase
    {
        public string _writeSideQueuePath;
        public string _readSideQueuePath;

        protected EventSourceFactory<SymbolEntity> Factory { get; private set; }
        private WriteSideServer<SymbolEntity> _writeSideProcessor;
        private ReadSideServer<SymbolEntity> _readSideProcessor;
        protected TestContextLogger _logger;

        public TestContext TestContext { get; set; }

        protected IDataAccessLayer<SymbolEntity> Dal { get; private set; }

        public void Initialize(int numberofWriteThreads, int numberofReadThreads, int? snapshotFrequency, int? snapShotSkewSeconds)
        {
            _logger = new TestContextLogger(TestContext);

            //Dal = new TableStorageDataAccessLayer<SymbolEntity>(Constants.StorageAccountName, Constants.StorageAccountKey);
            Dal = new InMemoryDataAccessLayer<SymbolEntity>();

            Factory = new EventSourceFactory<SymbolEntity>(
                Dal,
                new List<Type> { typeof(SymbolAgregate) }, 
                _logger,
                //new ServiceBusEventSourceClient(Constants.ServiceBusConnectionString, Constants.ServiceBusReadSideQueue), new ServiceBusEventSourceClient(Constants.ServiceBusConnectionString, Constants.ServiceBusWriteSideQueue)
                new InProcMessagingClient(), new InProcMessagingClient()
            );

            if(numberofWriteThreads > 0)
                _writeSideProcessor = Factory.CreateWriteSideServer(numberofWriteThreads);

            if (numberofReadThreads > 0)
                _readSideProcessor = Factory.CreateReadSideServer(numberofReadThreads, snapshotFrequency, snapShotSkewSeconds);
        }

        internal void WaitForClients(IMessagingClient writeClient, IMessagingClient readClient, long timeout)
        {
            DateTime start = DateTime.Now;
            bool stale = false;
            long currentWaitCount = writeClient.MessageWaitingCount() + readClient.MessageWaitingCount();
            while (currentWaitCount > 0 && !(stale && (DateTime.Now - start).TotalMilliseconds > timeout))
            {
                var beforeWaitCount = writeClient.MessageWaitingCount() + readClient.MessageWaitingCount();
                Thread.Sleep(1000);
                currentWaitCount = writeClient.MessageWaitingCount() + readClient.MessageWaitingCount();
                stale = beforeWaitCount == currentWaitCount;
            }                

            Assert.IsTrue(writeClient.MessageWaitingCount() == 0, "Write Client still have Message waiting after timeout.");
            Assert.IsTrue(readClient.MessageWaitingCount() == 0, "Read Client still have Message waiting after timeout.");
        }
    }
}
