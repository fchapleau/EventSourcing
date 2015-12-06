using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.InMemoryDal
{
    public class InProcMessagingClient : IMessagingClient
    {
        private Guid _queueId;

        public InProcMessagingClient()
        {
            _queueId = Guid.NewGuid();
        }

        public void SendMessage(EntityEvent e)
        {
            InProcMessagingServer.Instance.Enqueue(_queueId, e);
        }

        public IMessagingEvent Receive()
        {
            return InProcMessagingServer.Instance.Dequeue(_queueId);
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
