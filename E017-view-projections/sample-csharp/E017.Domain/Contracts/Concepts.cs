using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace E017.Contracts
{

    [Serializable]
    public sealed class FactoryId : AbstractIdentity<long>
    {
        public const string TagValue = "factory";

        public FactoryId(long id)
        {
            Contract.Requires(id > 0);
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override long Id { get; protected set; }

        public FactoryId() { }
    }
    [DataContract]
    public sealed class InventoryShipment
    {
        [DataMember(Order = 1)]
        public string Name { get; private set; }
        [DataMember(Order = 2)]
        public CarPart[] Cargo { get; private set; }

        InventoryShipment() {}

        public InventoryShipment(string name, CarPart[] cargo)
        {
            Name = name;
            Cargo = cargo;
        }
    }


    [DataContract]
    public sealed class CarPart
    {
        [DataMember(Order = 1)]
        public string Name { get; private set; }
        [DataMember(Order = 2)]
        public int Quantity { get; private set; }

        public CarPart(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    } 
}
