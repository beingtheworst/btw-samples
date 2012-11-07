using E014.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    public class produce_a_car : factory_application_service_spec
    {
        readonly FactoryId Id = new FactoryId(17);

        // Note: In Episode 14 of the BTW Podcast this test 
        // was referred to as "fry_not_assigned_to_factory" but has since been renamed.
        [Test]
        public void assigning_employee_not_in_factory_is_an_error()
        {
            Given(new FactoryOpened(Id));
            When(new ProduceACar(Id, "fry", "Ford"));

            Expect("unknown-employee");
        }

        // Note: In Episode 14 of the BTW Podcast this test 
        // was referred to as "part-not-found" but has since been renamed.
        [Test]
        public void missing_required_car_part_is_an_error()
        {

            // use TestBlueprintLibrary's RecordBlueprint method to create an Event that loads Domain Service 
            // with a blueprint for a "Ford" that returns, "1 chassis is needed to build it", that our test can use
            Given(
                     new FactoryOpened(Id),
                     Library.RecordBlueprint("Ford", new CarPart("chassis", 1)),
                     new EmployeeAssignedToFactory(Id, "fry")
                );
            When(new ProduceACar(Id, "fry", "Ford"));
            Expect("required-part-not-found");
        }

        [Test]
        public void car_model_not_in_blueprint_library_is_an_error()
        {
            Given(
                     new FactoryOpened(Id),
                     new EmployeeAssignedToFactory(Id, "fry")
                );
            When(new ProduceACar(Id, "fry", "Volvo"));
            Expect("car-model-not-found");
        }

        [Test]
        public void car_produced_announcment_received()
        {
            Given(
                    Library.RecordBlueprint("death star", new CarPart("magic box", 10)),
                    Library.RecordBlueprint("Ford", NewCarPartList("chassis", "4 wheels", "engine")),
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
            Given(
                    Library.RecordBlueprint("Ford", NewCarPartList("chassis", "4 wheels", "engine")),
                    new FactoryOpened(Id),
                    new EmployeeAssignedToFactory(Id, "fry"),
                    new ShipmentUnpackedInCargoBay(Id, "fry", new[] { NewShipment("ship-1", "chassis", "4 wheels", "engine") }),
                    new CarProduced(Id, "fry", "Ford", NewCarPartList("chassis", "4 wheels", "engine")),
                    new ShipmentUnpackedInCargoBay(Id, "fry", new[] { NewShipment("ship-2", "chassis", "4 wheels", "engine") })
                );

            When(new ProduceACar(Id, "fry", "Ford"));

            Expect("employee-already-produced-car-today");
        }


        [Test]
        public void when_factory_not_open_is_an_error()
        {
            When(new ProduceACar(Id, "fry", "Ford"));
            Expect("factory-is-not-open");
        }
    }
}
