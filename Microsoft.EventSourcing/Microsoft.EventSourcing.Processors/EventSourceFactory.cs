using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.Processors
{
    public class EventSourceFactory<T> where T : class, IEntity, new()
    {
        private Type _aggregateType;
        private ILogging _logger;
        private IMessagingClient _readSide;
        private IMessagingClient _writeSide;
        private IDataAccessLayer<T> DataAccessLayer { get; set; }
        public EventSourceFactory(IDataAccessLayer<T> dal, Type aggregateType, ILogging logger, IMessagingClient readSideClient, IMessagingClient writeSideClient)
        {
            DataAccessLayer = dal;
            _aggregateType = aggregateType;
            _logger = logger;
            _readSide = readSideClient;
            _writeSide = writeSideClient;
        }
        
        public ProjectionClient<T> CreateProjectionClient()
        {
            return new ProjectionClient<T>(DataAccessLayer);
        }

        public ReadSideServer<T> CreateReadSideServer(int numberOfThreads, int snapshotFrequency, int snapShotSkewSeconds)
        {
            return new ReadSideServer<T>(_aggregateType, _logger, DataAccessLayer, CreateReadSideClient(), numberOfThreads, snapshotFrequency, snapShotSkewSeconds);
        }
        public WriteSideServer<T> CreateWriteSideServer(int numberOfThreads)
        {
            return new WriteSideServer<T>(_logger, DataAccessLayer, CreateWriteSideClient(), CreateReadSideClient(), numberOfThreads);
        }

        public IMessagingClient CreateWriteSideClient()
        {
            return _writeSide;
        }

        public IMessagingClient CreateReadSideClient()
        {
            return _readSide;
        }
    }
}
