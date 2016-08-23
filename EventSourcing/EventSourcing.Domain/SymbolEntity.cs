﻿using EventSourcing;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests.Domain
{
    public class SymbolEntity : IEntity
    {
        public SymbolEntity() { }
        public Guid UId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }

        public void ReadEntity(IDictionary<string, TypedProperty> properties)
        {
            UId = properties["UId"].GuidValue;
            Name = properties["Name"].StringValue;
            Quantity = properties["Quantity"].Int32Value;
        }

        public IDictionary<string, TypedProperty> WriteEntity()
        {
            var toReturn = new Dictionary<string, TypedProperty>();
            toReturn.Add("UId", new TypedProperty(UId));
            toReturn.Add("Name", new TypedProperty(Name));
            toReturn.Add("Quantity", new TypedProperty(Quantity));
            return toReturn;
        }

        public override bool Equals(object obj)
        {
            var otherCasted = obj as SymbolEntity;

            if (otherCasted == null) return false;
            if (!otherCasted.UId.Equals(this.UId)) return false;
            if (!otherCasted.Name.Equals(this.Name)) return false;
            if (!otherCasted.Quantity.Equals(this.Quantity)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
