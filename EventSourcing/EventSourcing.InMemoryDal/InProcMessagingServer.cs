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

        private ConcurrentDictionary<Guid, ConcurrentQueue<InProcMessagingEvent>> _queues;

        private ConcurrentQueue<InProcMessagingEvent> GetQueue(Guid queueId)
        {
            Monitor.Enter(_queues);
            if (!_queues.ContainsKey(queueId))
                _queues.AddOrUpdate(queueId, new ConcurrentQueue<InProcMessagingEvent>(), (key, value) => new ConcurrentQueue<InProcMessagingEvent>());
            Monitor.Exit(_queues);

            return _queues[queueId];
        }

        private ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>> _inProgress;
        private ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>> _abandons;
        private ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>> _deadLetters;

        private InProcMessagingServer() 
        {
            _queues = new ConcurrentDictionary<Guid, ConcurrentQueue<InProcMessagingEvent>>();
            _inProgress = new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>();
            _abandons = new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>();
            _deadLetters = new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>();
            _sequence = 0;
        }

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

        internal void Complete(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            Monitor.Enter(_inProgress);
            if (!_inProgress.ContainsKey(queueId)) _inProgress.AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            Monitor.Exit(_inProgress);

            _inProgress[queueId].TryTake(out inProcMessagingEvent);
        }

        internal void DeadLetter(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            Monitor.Enter(_deadLetters);
            if (!_deadLetters.ContainsKey(queueId)) _deadLetters.AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            Monitor.Exit(_deadLetters);

            _deadLetters[queueId].Add(inProcMessagingEvent);
            Complete(queueId, inProcMessagingEvent);
        }

        internal void Abandon(Guid queueId, InProcMessagingEvent inProcMessagingEvent)
        {
            Monitor.Enter(_abandons);
            if (!_abandons.ContainsKey(queueId)) _abandons.AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            Monitor.Exit(_abandons);

            _abandons[queueId].Add(inProcMessagingEvent);
            Complete(queueId, inProcMessagingEvent);
        }
                
        internal IMessagingEvent Dequeue(Guid queueId)
        {
            
            var start = DateTime.Now;

            InProcMessagingEvent e = null;
            do
            {
                Monitor.Enter(_inProgress);
                GetQueue(queueId).TryDequeue(out e);
                if (e != null)
                {
                    if (!_inProgress.ContainsKey(queueId)) _inProgress.AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
                    _inProgress[queueId].Add(e);
                }
                Monitor.Exit(_inProgress);
                Thread.Sleep(100);
            }
            while (e == null && (DateTime.Now - start).Milliseconds < 10000);


            return e;
        }

        internal void Enqueue(Guid queueId, EntityEvent e)
        {
            Monitor.Enter(this);
            GetQueue(queueId).Enqueue(new InProcMessagingEvent(this, e, _sequence++, queueId));
            Monitor.Exit(this);
        }

        internal long Count(Guid queueId)
        {
            Monitor.Enter(_inProgress);
            if (!_inProgress.ContainsKey(queueId)) _inProgress.AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            Monitor.Exit(_inProgress);

            return GetQueue(queueId).Count() + _inProgress[queueId].Count();
        }

        internal long ErrorCount(Guid _queueId)
        {
            return _deadLetters.Sum(f => f.Value.Count()) + _abandons.Sum(f => f.Value.Count());
        }
    }
}
