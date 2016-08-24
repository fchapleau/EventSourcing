using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.InMemoryDal
{
    public enum ExclusiveBagsName
    {
        InProgress,
        Abandon,
        DeadLetter
    }

    public class ExclusivityManager
    {
        private Dictionary<ExclusiveBagsName, ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>> _bags;
        private ConcurrentDictionary<Guid, ConcurrentQueue<InProcMessagingEvent>> _queue;

        public ExclusivityManager()
        {
            _bags = new Dictionary<ExclusiveBagsName, ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>>();
            _bags.Add(ExclusiveBagsName.Abandon, new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>());
            _bags.Add(ExclusiveBagsName.DeadLetter, new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>());
            _bags.Add(ExclusiveBagsName.InProgress, new ConcurrentDictionary<Guid, ConcurrentBag<InProcMessagingEvent>>());

            _queue = new ConcurrentDictionary<Guid, ConcurrentQueue<InProcMessagingEvent>>();
        }

        internal void AddQueue(Guid queueId)
        {
            _bags[ExclusiveBagsName.InProgress].AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            _bags[ExclusiveBagsName.Abandon].AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());
            _bags[ExclusiveBagsName.DeadLetter].AddOrUpdate(queueId, new ConcurrentBag<InProcMessagingEvent>(), (key, value) => new ConcurrentBag<InProcMessagingEvent>());

            _queue.AddOrUpdate(queueId, new ConcurrentQueue<InProcMessagingEvent>(), (key, value) => new ConcurrentQueue<InProcMessagingEvent>());
        }

        public ExclusiveDictionnary<Guid, ConcurrentQueue<InProcMessagingEvent>> GetExclusiveQueue()
        {
            return new ExclusiveDictionnary<Guid, ConcurrentQueue<InProcMessagingEvent>>(_queue, true);
        }

        public ExclusiveDictionnary<Guid, ConcurrentQueue<InProcMessagingEvent>> GetNonExclusiveQueue()
        {
            return new ExclusiveDictionnary<Guid, ConcurrentQueue<InProcMessagingEvent>>(_queue, false);
        }

        public ExclusiveDictionnary<Guid, ConcurrentBag<InProcMessagingEvent>> GetExclusiveBag(ExclusiveBagsName name)
        {
            return new ExclusiveDictionnary<Guid, ConcurrentBag<InProcMessagingEvent>>(_bags[name], true);
        }
        public ExclusiveDictionnary<Guid, ConcurrentBag<InProcMessagingEvent>> GetNonExclusiveBag(ExclusiveBagsName name)
        {
            return new ExclusiveDictionnary<Guid, ConcurrentBag<InProcMessagingEvent>>(_bags[name], false);
        }
    }
}
