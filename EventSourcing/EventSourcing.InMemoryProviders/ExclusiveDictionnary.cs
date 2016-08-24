using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.InMemoryDal
{
    public class ExclusiveDictionnary<T, T2> : IDisposable where T2 : new()
    {
        private bool _exclusive;
        public ConcurrentDictionary<T, T2> Items { get; private set; }

        public ExclusiveDictionnary(ConcurrentDictionary<T, T2> bag, bool exclusive)
        {
            Items = bag;
            _exclusive = exclusive;
            if (exclusive)
                Monitor.Enter(Items);
        }
        public T2 GetById(T id)
        {
            if (!Items.ContainsKey(id))
                throw new ArgumentOutOfRangeException();                
            return Items[id];
        }

        public void Dispose()
        {
            if(_exclusive)
                Monitor.Exit(Items);
        }
    }
}
