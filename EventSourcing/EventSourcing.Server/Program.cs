using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventSourcing.Processors;
using EventSourcing.ServiceBusMessaging;
using EventSourcing.Tests.Domain;
using EventSourcing.Tests.Common;

namespace EventSourcing.Tests.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0) return;

            var logger = new ConsoleLogger();

            var Factory = new EventSourceFactory<SymbolEntity>(
                new TableStorageDataAccessLayer<SymbolEntity>(Constants.StorageAccountName, Constants.StorageAccountKey),
                new List<Type> { typeof(SymbolAgregate) },
                logger,
                new ServiceBusEventSourceClient(Constants.ServiceBusConnectionString, Constants.ServiceBusReadSideQueue),
                new ServiceBusEventSourceClient(Constants.ServiceBusConnectionString, Constants.ServiceBusWriteSideQueue)
                );

            WriteSideServer<SymbolEntity> writeSideProcessor = null;
            ReadSideServer<SymbolEntity> readSideProcessor = null;

            if (args[0] == "WriteSide") writeSideProcessor = Factory.CreateWriteSideServer(int.Parse(args[1]));
            if (args[0] == "ReadSide") readSideProcessor = Factory.CreateReadSideServer(int.Parse(args[1]), int.Parse(args[2]), 3);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
