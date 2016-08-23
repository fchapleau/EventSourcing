using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.ServiceBusMessaging
{
    public class ServiceBusEventSourceClient : IMessagingClient
    {
        private string _serviceBusConnectionString;
        private string _queuePath;

        private MessageReceiver _receiver;

        private MessageReceiver Receiver
        {
            get
            {
                if (_receiver == null)
                {
                    var mf = MessagingFactory.CreateFromConnectionString(_serviceBusConnectionString);
                    _receiver = mf.CreateMessageReceiver(_queuePath);
                }
                return _receiver;
            }
        }
        public ServiceBusEventSourceClient(string serviceBusConnectionString, string queuePath)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _queuePath = queuePath;
        }
        public void SendMessage(EntityEvent e)
        {
            var mf = MessagingFactory.CreateFromConnectionString(_serviceBusConnectionString);
            var writeSideClient = mf.CreateQueueClient(_queuePath);
            writeSideClient.Send(new BrokeredMessage(e));
        }

        public IMessagingEvent Receive(TimeSpan timeout)
        {
            return new ServiceBusMessagingEvent(Receiver.Receive(timeout));
        }

        public long MessageWaitingCount()
        {
            var ns = NamespaceManager.CreateFromConnectionString(_serviceBusConnectionString);
            var queue = ns.GetQueue(_queuePath);
            var details = queue.MessageCountDetails;
            return details.ActiveMessageCount;
        }

        public long MessageInErrorCount()
        {
            return 0;
        }
    }
}
