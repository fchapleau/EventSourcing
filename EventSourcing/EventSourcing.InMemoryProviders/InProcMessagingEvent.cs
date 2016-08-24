using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.InMemoryDal
{
    public class InProcMessagingEvent : IMessagingEvent
    {
        private InProcMessagingServer _server;
        private EntityEvent _event;
        private long _sequenceNumber;
        private Guid _queueId;

        internal InProcMessagingEvent(InProcMessagingServer server, EntityEvent e, long sequenceNumber, Guid queueId)
        {
            _server = server;
            _event = e;
            _sequenceNumber = sequenceNumber;
            _queueId = queueId;
        }
        public EntityEvent Event
        {
            get { return _event; }
        }

        public long SequenceNumber
        {
            get { return _sequenceNumber; }
        }

        public void Complete()
        {
            _server.Complete(_queueId, this);
        }

        public void DeadLetter()
        {
            _server.DeadLetter(_queueId, this);
        }

        public void Abandon()
        {
            _server.Abandon(_queueId, this);
        }

        public void Dispose()
        {
            //nothing to do;
        }
    }
}
