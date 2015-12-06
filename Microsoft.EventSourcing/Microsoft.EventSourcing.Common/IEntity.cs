using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EventSourcing
{
    public interface IEntity : ICloneable  
    {
        Guid UId { get; set; }
        void ReadEntity(IDictionary<string, EntityProperty> properties);
        IDictionary<string, EntityProperty> WriteEntity();
    }
}
