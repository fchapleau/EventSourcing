using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.InMemoryDal
{
    public class InProcMessagingClient : IMessagingClient
    {
        private Guid _queueId;

        public InProcMessagingClient()
        {
            _queueId = Guid.NewGuid();
            InProcMessagingServer.Instance.AddQueue(_queueId);
        }

        public void SendMessage(EntityEvent e)
        {
            InProcMessagingServer.Instance.Enqueue(_queueId, e);
        }

        public IMessagingEvent Receive(TimeSpan timeout)
        {
            return InProcMessagingServer.Instance.Dequeue(_queueId, timeout);
        }

        public long MessageWaitingCount()
        {
            return InProcMessagingServer.Instance.Count(_queueId);
        }

        public long MessageInErrorCount()
        {
            return InProcMessagingServer.Instance.ErrorCount(_queueId);
        }
    }
}
