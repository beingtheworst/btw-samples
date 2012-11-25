using System.Collections.Generic;
using E017.Contracts;

namespace E017.Projections
{
    public sealed class InventoryProjection
    {

        public IDictionary<string,int> Parts = new Dictionary<string, int>(); 

        void Change(string name, int delta)
        {
            int count;
            if (!Parts.TryGetValue(name, out count))
            {
                count = 0;
            }
            Parts[name] = count + delta;
        }

        public void When(ShipmentReceivedInCargoBay e)
        {
            foreach (var carPart in e.Shipment.Cargo)
            {
                Change(carPart.Name, carPart.Quantity);
            }
        }

        public void When(CarProduced e)
        {
            foreach (var part in e.Parts)
            {
                Change(part.Name, -part.Quantity);
            }
        }
    }
}