using System.Linq;
using System.Text;

namespace E017.Contracts
{
    /// <summary>
    /// Provides external "ToString" representations for messages which can't be
    /// handled by inline explicit synatax of DDD.
    /// </summary>
    static class Describe
    {
        public static string Message(ShipmentReceivedInCargoBay e)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Shipment '{0}' transferred to cargo bay:", e.Shipment.Name).AppendLine();
            foreach (var carPart in e.Shipment.Cargo)
            {
                builder.AppendFormat("     {0} {1} pcs", carPart.Name, carPart.Quantity).AppendLine();
            }
            return builder.ToString();
        }

        public static string Message(ReceiveShipmentInCargoBay e)
        {
            // TODO:  Need to fix this now that it uses InventoryShipment shipment instead of parts array.
            //return string.Format(@"Received shipment '{0}' in cargo bay:{1}"
            //        , e.ShipmentName
            //        , e.Parts.Aggregate("", (x, y) => x + string.Format("\r\n{0}:{1}", y.Name, y.Quantity))
            //    );

            return string.Format(@"Received shipment '{0}' in cargo bay:{1}"
                    , e.ShipmentName
                    , "TODO: Need to Show Part In Inventory"
                );
        }

        public static string Message(CarProduced e)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("produce car {0}:", e.CarModel).AppendLine();
            foreach (var carPart in e.Parts)
                builder.AppendFormat("     {0} {1} pcs", carPart.Name, carPart.Quantity).AppendLine();

            return builder.ToString();

        }

        public static string Message(ShipmentUnpackedInCargoBay e)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} unload:", e.EmployeeName).AppendLine();
            foreach (var inventoryShipment in e.InventoryShipments)
            {
                builder.AppendFormat("\tshipment name '{0}' and parts:\r\n", inventoryShipment.Name);
                foreach (var carPart in inventoryShipment.Cargo)
                    builder.AppendFormat("\t\t{0} {1} pcs", carPart.Name, carPart.Quantity).AppendLine();
            }

            return builder.ToString();
        }

    }
}
