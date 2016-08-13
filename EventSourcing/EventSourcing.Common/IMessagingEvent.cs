using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IMessagingEvent : IDisposable
    {
        EntityEvent Event { get; }
        long SequenceNumber { get; }
        void Complete();
        void DeadLetter();
        void Abandon();

    }
}
