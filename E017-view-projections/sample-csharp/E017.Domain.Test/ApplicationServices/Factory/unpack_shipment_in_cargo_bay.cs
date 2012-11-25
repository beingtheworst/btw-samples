using E017.Contracts;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace E017.Domain.ApplicationServices.Factory
{
    

    public class unpack_shipment_in_cargo_bay : factory_application_service_spec
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
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "ben"),
                  new ShipmentReceivedInCargoBay(Id, NewShipment("ship-1", "chassis")));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            ExpectError("unknown-employee");
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
            ExpectError("employee-already-unpacked-cargo");
        }

        [Test]
        public void it_is_an_error_if_there_are_no_shipments_to_unpack()
        {
            Given(new FactoryOpened(Id),
                  new EmployeeAssignedToFactory(Id, "fry"));
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            ExpectError("empty-InventoryShipments");
        }

        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new UnpackAndInventoryShipmentInCargoBay(Id, "fry"));
            ExpectError("factory-is-not-open");
        }
    }
}
