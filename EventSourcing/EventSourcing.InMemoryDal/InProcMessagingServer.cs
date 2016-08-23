using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.InMemoryDal
{
    internal class InProcMessagingServer
    {
        private long _sequence;
        private ExclusivityManager _bagManager;

        private InProcMessagingServer() 
        {
            _bagManager = new ExclusivityManager();
            _sequence = 0;
        }

        internal void AddQueue(Guid queueId)
        {
            _bagManager.AddQueue(queueId);
        }

        #region Instance

        private static InProcMessagingServer _server;
        public static InProcMessagingServer Instance
        {
            get
            {
                if (_server == null)
                {
                    var server = new InProcMessagingServer();
                    _server = server;
                }
                return _server;
            }
        }

        #endregion

        #region Complete / Abandon / Deadletter / ErrorCount

        internal void Complete(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            using (var bag = _bagManager.GetNonExclusiveBag(ExclusiveBagsName.InProgress))
            {
                var queue =  bag.GetById(queueId);
                queue.TryTake(out inProcMessagingEvent);
            }
        }

        internal void Abandon(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            using (var bag = _bagManager.GetNonExclusiveBag(ExclusiveBagsName.Abandon))
            {
                var queue = bag.GetById(queueId);
                queue.Add(inProcMessagingEvent);
            }
            
            Complete(queueId, inProcMessagingEvent);
        }

        internal void DeadLetter(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            using (var bag = _bagManager.GetNonExclusiveBag(ExclusiveBagsName.DeadLetter))
            {
                var queue = bag.GetById(queueId);
                queue.Add(inProcMessagingEvent);
            }

            Complete(queueId, inProcMessagingEvent);
        }
        
        internal long ErrorCount(Guid _queueId)
        {
            using (var deadLetters = _bagManager.GetExclusiveBag(ExclusiveBagsName.DeadLetter))
            {
                using (var abandons = _bagManager.GetExclusiveBag(ExclusiveBagsName.Abandon))
                {
                    return deadLetters.Items.Sum(f => f.Value.Count()) + abandons.Items.Sum(f => f.Value.Count());
                }
            }
        }

        #endregion

        #region Queuing Implementation

        internal IMessagingEvent Dequeue(Guid queueId, TimeSpan timeout)
        {
            
            var start = DateTime.Now;

            InProcMessagingEvent e = null;
            do
            {
                using (var queue = _bagManager.GetExclusiveQueue())
                {                        
                    if (queue.GetById(queueId).TryDequeue(out e))
                    {
                        using (var inprogress = _bagManager.GetExclusiveBag(ExclusiveBagsName.InProgress))
                        {
                            inprogress.GetById(queueId).Add(e);
                        }
                    }
                }
                if(e==null)
                    Thread.Sleep(100);
            }
            while (e == null && (DateTime.Now - start) < timeout);

            return e;
        }

        internal void Enqueue(Guid queueId, EntityEvent e)
        {
            using (var queue = _bagManager.GetNonExclusiveQueue())
            {
                queue.GetById(queueId).Enqueue(new InProcMessagingEvent(this, e, _sequence++, queueId));
            }
        }

        internal long Count(Guid queueId)
        {
            using (var queue = _bagManager.GetExclusiveQueue())
            {
                using (var bag = _bagManager.GetExclusiveBag(ExclusiveBagsName.InProgress))
                {
                    return queue.GetById(queueId).Count() + bag.GetById(queueId).Count();
                }
            }
        }

        #endregion

    }
}
