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

        // The projections in this sample are simplified for now and combine the in-memory state and the projection code.
        // This is a simple way to have this projection populate its View (i.e. often a persistent read model).
        // The View is an eventually consistent snapshot of specific Event states
        // that consumers can read/use to answer their queries (this can be used for the "Q"uery side of C"Q"RS by the way)

        // Hey look, using a simple dictionary for now to update the sate of the in-memory "View".
        // In production code we will usually load a document from the repository to get the actual persisted View's state
        // make updates to it as we react to the Events we care about below, and then write the new state of the View back to the repository.
        public IDictionary<FactoryId,FactoryInfo> Factories = new Dictionary<FactoryId, FactoryInfo>();

        public sealed class FactoryInfo
        {
            public int WorkerCount;
            public int PartsInCargoBay;
        }


        // the approach below is VERY similar to how the FactoryState class reacts to 
        // Events to update the state of the FactoryAggregate itself

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