﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IEntity: ICloneable  
    {
        Guid UId { get; set; }
        void ReadEntity(IDictionary<string, TypedProperty> properties);
        IDictionary<string, TypedProperty> WriteEntity();
    }
}
