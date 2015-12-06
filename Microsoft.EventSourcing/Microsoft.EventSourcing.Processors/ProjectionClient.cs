using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing.Processors
{
    public class ProjectionClient<T> where T : IEntity, new()
    {
        private IDataAccessLayer<T> _dal;
        internal ProjectionClient(IDataAccessLayer<T> dal)
        {
            _dal = dal;
        }

        public T GetEntity(Guid uId)
        {
            return (T)_dal.GetEntity(uId);
        }
    }
}
