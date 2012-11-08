using E014.Contracts;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    

    public class unpack_and_inventory_shipment_in_cargo_bay : factory_application_service_spec
    {
        static readonly FactoryId Id = new FactoryId(25);

        [Test]
        public void an_unpacked_announcement_is_made_with_correct_inventory_list()
        {
            var shipment = NewShipment("ship-1", "chassis");

            Given(new FactoryOpened(Id),
                            new EmployeeAssignedToFactory(Id, "fry"),
                            new ShipmentReceivedInCargoBay(Id, shipment));

            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect(new ShipmentUnpackedInCargoBay(Id, "fry", new[] { shipment }));
        }

        [Test]
        public void multiple_shipments_are_unpacked_properly()
        {
            var shipment1 = NewShipment("ship-1", "chassis");
            var shipment2 = NewShipment("ship-2", "engine");

            Given(new FactoryOpened(Id),
                new EmployeeAssignedToFactory(Id, "fry"),
                new ShipmentReceivedInCargoBay(Id, shipment1),
                new ShipmentReceivedInCargoBay(Id, shipment2));

            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect(new ShipmentUnpackedInCargoBay(Id, "fry", new[]
                {
                    shipment1,
                    shipment2
                }));
        }

        [Test]
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "ben"),
                  new ShipmentReceivedInCargoBay(Id, NewShipment("ship-1", "chassis")));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("unknown-employee");
        }

        [Test]
        public void an_employee_asked_to_unpack_more_than_once_a_day_is_not_allowed()
        {
            var shipment = NewShipment("ship-1", "chassis");

            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"),
                  new ShipmentReceivedInCargoBay(Id, shipment),
                  new ShipmentUnpackedInCargoBay(Id, "fry", new[] { shipment }));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("employee-already-unpacked-cargo");
        }

        [Test]
        public void it_is_an_error_if_there_are_no_shipments_to_unpack()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("empty-InventoryShipments");
        }

        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            Expect("factory-is-not-open");
        }
    }
}
