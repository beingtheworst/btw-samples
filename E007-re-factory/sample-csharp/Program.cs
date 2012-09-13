using System;
using System.Reflection;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E007_re_factory
{

    // first we refactor our factory from previous episode a little bit.
    // 1. rename TheFactoryJournal into -> Changes
    // 2. move state variables into a separate class (so that we can't accidentally touch it)
    // 3. allow loading this state variable from a journal
    // 4. add constructors and [Serializable] attribute to all events
    // this code is located inside "factory.cs"
    
    // then we start writing short "specification" stories, using C# syntax
    // the specs below are written as runnable NUnit tests and/or you can see similar
    // test output displayed to the console in this sample (Ctrl+F5).
    // to see this in NUnit using Visual Studio, try right-clicking on the E005-testing-use-cases project
    // and navigate to Test With --> NUnit and click "Run" in the NUnit test runner to see the test output

    // this was the first specification example mentioned in podcast Episode 5
    // typically one specification class like this is used per aggregate command method
    // this class grouping of specifications and use cases that test each method is also called a "Test Fixture"
    // each [Test] specification covering a use case is also called a "Unit Test"
    // in this case, testing the behavior of the "TransferShipmentToCargoBay" method on the factory aggregate
    // and the serveral use cases related to that command method
    public sealed class when_transfer_shipment_to_cargo_bay : factory_specs
    {
        // Use Case: Empty Shipment
        [Test]
        public void empty_shipment()
        {
            // You will notice the use of the "Given, When, Then" structure inside specifications

            // 
            Given = new IEvent[] { new EmployeeAssignedToFactory("yoda")  };

            // When (we do something with a method, like transfer a shipment to the cargo bay)
            When = f => f.TransferShipmentToCargoBay("some shipment", new CarPart[0]);
            ThenException = ex => ex.Message.Contains("Empty shipments are not accepted!");
        }

        // Use Case: Empty Shipment and No Workers at the Factory
        [Test]
        public void empty_shipment_comes_to_empty_factory()
        {
            // Instead of some empty array of events like: Given = new IEvent[];
            // It is just left out, which does the same thing:  No Pre-Existing Events Yet!
            // So: Given NO EVENTS

            When = f => f.TransferShipmentToCargoBay("some shipment", new[] {new CarPart("chassis", 1)});
            ThenException = ex => ex.Message.Contains("has to be somebody at factory");
        }
        [Test]
        public void there_already_are_two_shipments()
        {
            Given = new IEvent[]
                {
                    new EmployeeAssignedToFactory("chubakka"), 
                    new ShipmentTransferredToCargoBay("shipmt-11", new CarPart("engine",3)), 
                    new ShipmentTransferredToCargoBay("shipmt-12", new CarPart("wheels", 40)), 
                };
            When = f => f.TransferShipmentToCargoBay("shipmt-13", new CarPart("bmw6", 20));
            ThenException = ex => ex.Message.Contains("More than two shipments can't fit");
        }
    }

    // and another test fixture to test the behavior of the "AssignEmployeeToFactory" method
    public sealed class when_assign_employee_to_factory : factory_specs
    {
        [Test]
        public void empty_factory()
        {
            // Given no events
            When = f => f.AssignEmployeeToFactory("fry");
            Then = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
        }
        [Test]
        public void fry_is_assigned_to_factory()
        {
            Given = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
            When = f => f.AssignEmployeeToFactory("fry");
            ThenException = ex => ex.Message.Contains("only one employee can have");
        }
        [Test]
        public void bender_comes_to_empty_factory()
        {
            When = f => f.AssignEmployeeToFactory("bender");
            ThenException = ex => ex.Message.Contains("Guys with name 'bender' are trouble");
        }
    }

    // Homework - Cover existing E004 homework code with specs/tests
    public sealed class when_unload_shipment_from_cargo_bay : factory_specs
    {
        // Rule: Are there actually shipments to unload (cargo bay not empty)?
        [Test]
        public void no_shipments_to_unload()
        {
            // Basically, event history has no record that a ShipmentTransferredToCargoBay has happened
            
            // Given = new IEvent[]; (there are no events)
            When = f => f.UnloadShipmentFromCargoBay("darth");
            ThenException = ex => ex.Message.Contains("There are no shipments to unload in this cargo bay");
        }

        // Rule: Employee can only unload if cargo bay not empty and
        // employee hasn't already unloaded the cargo bay today
        [Test]
        public void employee_has_already_unloaded_cargo_bay_today()
        {
            Given = new IEvent[]
                {
                    new ShipmentTransferredToCargoBay("shipment-20", new CarPart("flux capacitor", 1)),
                    new ShipmentUnloadedFromCargoBay("yoda")
                };
            When = f => f.UnloadShipmentFromCargoBay("yoda");
            ThenException = ex => ex.Message.Contains("has already unloaded a cargo bay today");
        }
    }

    public sealed class when_produce_car : factory_specs
    {
        // Rule: Model T is the only car type that we can currently produce.
        [Test]
        public void car_model_not_model_t()
        {
            When = f => f.ProduceCar("luke", "Model A");
            ThenException = ex => ex.Message.Contains("is not a car we can make.");
        }

        // Rule: if we have an employee available that has not produced a car
        [Test]
        public void employee_has_already_produced_a_car_today()
        {
            Given = new IEvent[]
                {
                    new CarProduced("jabba", "Model T")
                };
            When = f => f.ProduceCar("jabba", "Model T");
            ThenException = ex => ex.Message.Contains("has already produced a car today");
        }

        // Rule:  Parts Needed To Build a Model T
        // 6 wheels
        // 1 engine
        // 2 sets of "bits and pieces"

        [Test]
        public void not_enough_wheels_for_model_t()
        {
            Given = new IEvent[]
                {
                    // Need 6 wheels for Model T
                    new ShipmentTransferredToCargoBay("shipmt-12", new CarPart("wheels", 5)),
                    new ShipmentTransferredToCargoBay("shipmt-30", new CarPart("engine",1)), 
                    new ShipmentTransferredToCargoBay("shipmt-31", new CarPart("bits and pieces", 40))
                };
            When = f => f.ProduceCar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("do not have enough parts to build");

        }
        
        [Test]
        public void not_enough_engines_for_model_t()
        {
            Given = new IEvent[]
                {
                    new ShipmentTransferredToCargoBay("shipmt-12", new CarPart("wheels", 6)),
                    // Need 1 engine for Model T
                    new ShipmentTransferredToCargoBay("shipmt-30", new CarPart("engine",0)), 
                    new ShipmentTransferredToCargoBay("shipmt-31", new CarPart("bits and pieces", 2))
                };
            When = f => f.ProduceCar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("do not have enough parts to build");

        }
        [Test]
        public void not_enough_bits_and_pieces_for_model_t()
        {
            Given = new IEvent[]
                {
                    new ShipmentTransferredToCargoBay("shipmt-12", new CarPart("wheels", 6)),
                    new ShipmentTransferredToCargoBay("shipmt-30", new CarPart("engine",1)),
                    // Need 2 bits and pieces for Model T
                    new ShipmentTransferredToCargoBay("shipmt-31", new CarPart("bits and pieces", 1))
                };
            When = f => f.ProduceCar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("do not have enough parts to build");

        }
        //[Test]
        //public void have_available_employee_and_parts_for_model_t()
        //{
        //    // This is a SUCCESS test

        //    // TODO: Some of these events don't really have to exist in current ProduceCar implementation
        //    // but probably should refactor so they have to

        //    Given = new IEvent[]
        //        {
        //            new EmployeeAssignedToFactory("chubakka"),
        //            // Rule:  Parts Needed To Build a Model T
        //            // 6 wheels
        //            // 1 engine
        //            // 2 sets of "bits and pieces"
        //            new ShipmentTransferredToCargoBay("shipmt-12", new CarPart("wheels", 6)),
        //            new ShipmentTransferredToCargoBay("shipmt-30", new CarPart("engine",1)),
        //            new ShipmentTransferredToCargoBay("shipmt-31", new CarPart("bits and pieces", 2)),
        //            new ShipmentUnloadedFromCargoBay("luke"),
        //            new CarProduced("luke", "Model T")
        //        };
        //    When = f => f.ProduceCar("chubakka", "Model T");

        //    // THEN
        //    // TODO:  What is syntax for "Then"?
        //    //   
    }


    public class Program
    {
        public static void Main()
        {
            // you can run these tests from NUnit, but if you don't
            // have it, we'll run it in a console as well
            RunSpecification(new when_assign_employee_to_factory());
            RunSpecification(new when_transfer_shipment_to_cargo_bay());
            // Homework
            RunSpecification(new when_unload_shipment_from_cargo_bay());
            RunSpecification(new when_produce_car());
        }

        static void RunSpecification(factory_specs specification)
        {
            Console.WriteLine(new string('=', 80));
            var cases = specification.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Print(ConsoleColor.DarkGreen, "Specification: {0}", specification.GetType().Name.Replace('_',' '));
            foreach (var methodInfo in cases)
            {
                Console.WriteLine(new string('-', 80));
                Print(ConsoleColor.DarkBlue, "Use case: {0}", methodInfo.Name.Replace("_", " "));
                Console.WriteLine();
                try
                {
                    specification.Setup();
                    methodInfo.Invoke(specification, null);
                    Print(ConsoleColor.DarkGreen, "\r\nPASSED!");
                }
                catch(Exception ex)
                {
                    Print(ConsoleColor.DarkRed, "\r\nFAIL! " + ex.Message);
                }
            }
        }

        static void Print(ConsoleColor color, string format, params object[] args)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = old;
        }
    }
}
