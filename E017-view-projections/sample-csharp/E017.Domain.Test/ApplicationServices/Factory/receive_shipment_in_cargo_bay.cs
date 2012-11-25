using E017.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E017.Domain.ApplicationServices.Factory
{
    public class receive_shipment_in_cargo_bay : factory_application_service_spec
    {
        static readonly FactoryId Id = new FactoryId(52);

        [Test]
        public void a_shipment_received_announcement_is_made_with_correct_car_parts_list()
        {
            Given(new FactoryOpened(Id), 
                    new EmployeeAssignedToFactory(Id, "yoda"));
            When(new ReceiveShipmentInCargoBay(Id, "shipment-777", NewCarPartList("engine")));
            Expect(new ShipmentReceivedInCargoBay(Id, NewShipment("shipment-777","engine")));
        }


        [Test]
        public void empty_shipment_is_not_allowed()
        {
            Given(new FactoryOpened(Id),
                            new EmployeeAssignedToFactory(Id, "yoda"));
            When(new ReceiveShipmentInCargoBay(Id, "some shipment", new CarPart[0]));
            ExpectError("empty-InventoryShipments");
        }

        [Test]
        public void an_empty_shipment_that_comes_to_factory_with_no_employees_is_not_received()
        {
            Given(new FactoryOpened(Id));
            When(new ReceiveShipmentInCargoBay(Id, "some shipment", NewCarPartList("chassis")));
            ExpectError("unknown-employee");
        }
        [Test]
        public void there_are_already_two_shipments_in_cargo_bay_so_no_new_shipments_allowed()
        {
            Given(
                new FactoryOpened(Id),
                    new EmployeeAssignedToFactory(Id, "chubakka"),
                    new ShipmentReceivedInCargoBay(Id, NewShipment("shipmt-11", "3 engine")),
                    new ShipmentReceivedInCargoBay(Id, NewShipment("shipmt-12", "40 wheel"))
                );

            When(new ReceiveShipmentInCargoBay(Id, "shipmt-13", NewCarPartList("20 bmw-6")));
            ExpectError("more-than-two-InventoryShipments");
        }

        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new ReceiveShipmentInCargoBay(Id, "some shipment", new CarPart[0]));
            ExpectError("factory-is-not-open");
        }
    }
}
