using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.ServiceBusMessaging
{
    public class ServiceBusMessagingEvent : IMessagingEvent
    {
        private BrokeredMessage _message;
        private EntityEvent _event;
        public ServiceBusMessagingEvent(BrokeredMessage message)
        {
            _message = message;
        }
        public EntityEvent Event
        {
            get
            {
                if(_event == null)
                    _event = _message.GetBody<EntityEvent>();
                return _event;
            }
        }

        public void Complete()
        {
            _message.Complete();
        }

        public void DeadLetter()
        {
            _message.DeadLetter();
        }

        public void Abandon()
        {
            _message.Abandon();
        }

        public void Dispose()
        {
            _message.Dispose();
        }


        public long SequenceNumber
        {
            get
            {
                return _message.SequenceNumber;
            }
        }
    }
}
