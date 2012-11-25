using System.Collections.Generic;
using System.Linq;
using E017.Contracts;

namespace E017.Projections
{
    /// <summary>
    /// Simplified (for now) projection with in-memory storage
    /// </summary>
    public sealed class ActiveFactoriesProjection
    {
        public IDictionary<FactoryId,FactoryInfo> Factories = new Dictionary<FactoryId, FactoryInfo>();

        public sealed class FactoryInfo
        {
            public int WorkerCount;
            public int PartsInCargoBay;
        }


        public void When(FactoryOpened e)
        {
            Factories[e.Id] = new FactoryInfo();
        }
        public void When(EmployeeAssignedToFactory e)
        {
            Factories[e.Id].WorkerCount += 1;
        }
        public void When(ShipmentReceivedInCargoBay e)
        {
            Factories[e.Id].PartsInCargoBay += e.Shipment.Cargo.Sum(p => p.Quantity);
        }
        public void When(ShipmentUnpackedInCargoBay e)
        {
            Factories[e.Id].PartsInCargoBay -= e.InventoryShipments.Sum(s => s.Cargo.Sum(p => p.Quantity));
        }
    }
}