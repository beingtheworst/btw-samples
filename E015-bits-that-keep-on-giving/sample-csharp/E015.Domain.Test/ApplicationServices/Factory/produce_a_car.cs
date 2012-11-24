using E015.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E015.Domain.ApplicationServices.Factory
{
    public class produce_a_car : factory_application_service_spec
    {
        readonly FactoryId Id = new FactoryId(17);

        [Test]
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(Id));
            When(new ProduceACar(Id, "fry", "Ford"));

            ExpectError("unknown-employee");
        }

        [Test]
        public void missing_required_car_part_is_an_error()
        {
            GivenSetup(Library.RecordBlueprint("Ford", new CarPart("chassis", 1)));
            Given(
                     new FactoryOpened(Id),
                     new EmployeeAssignedToFactory(Id, "fry")
                );
            When(new ProduceACar(Id, "fry", "Ford"));
            ExpectError("required-part-not-found");
        }

        [Test]
        public void car_model_not_in_blueprint_library_is_an_error()
        {
            Given(
                     new FactoryOpened(Id),
                     new EmployeeAssignedToFactory(Id, "fry")
                );
            When(new ProduceACar(Id, "fry", "Volvo"));
            ExpectError("car-model-not-found");
        }

        [Test]
        public void car_produced_announcment_received()
        {
            GivenSetup(
                Library.RecordBlueprint("death star", new CarPart("magic box", 10)),
                Library.RecordBlueprint("Ford", NewCarPartList("chassis", "4 wheels", "engine")));
            Given(
                    new FactoryOpened(Id),
                    new EmployeeAssignedToFactory(Id, "fry"),
                    new ShipmentUnpackedInCargoBay(Id, "fry", new[] { NewShipment("ship1", "chassis", "4 wheels", "engine") })
                );

            When(new ProduceACar(Id, "fry", "Ford"));

            Expect(new CarProduced(Id, "fry", "Ford", NewCarPartList("chassis", "4 wheels", "engine")));
        }


        [Test]
        public void an_employee_who_has_already_produced_a_car_today_cant_be_assigned()
        {
            GivenSetup(Library.RecordBlueprint("Ford", NewCarPartList("chassis", "4 wheels", "engine")));
            Given(
                    new FactoryOpened(Id),
                    new EmployeeAssignedToFactory(Id, "fry"),
                    new ShipmentUnpackedInCargoBay(Id, "fry", new[] { NewShipment("ship-1", "chassis", "4 wheels", "engine") }),
                    new CarProduced(Id, "fry", "Ford", NewCarPartList("chassis", "4 wheels", "engine")),
                    new ShipmentUnpackedInCargoBay(Id, "fry", new[] { NewShipment("ship-2", "chassis", "4 wheels", "engine") })
                );

            When(new ProduceACar(Id, "fry", "Ford"));

            ExpectError("employee-already-produced-car-today");
        }


        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new ProduceACar(Id, "fry", "Ford"));
            ExpectError("factory-is-not-open");
        }
    }
}
