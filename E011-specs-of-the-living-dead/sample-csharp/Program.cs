using System;
using System.Reflection;
using NUnit.Framework;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming
namespace E011_specs_of_the_living_dead
{
    # region This C# Project's Evolutionary Record
    // first we refactor our Factory from previous episode a little bit.
    // 1. rename TheFactoryJournal into -> Changes --> and Now --> "EventsThatHappened"
    // 2. move state variables into a separate class (so that we can't accidentally touch it)
    // 3. allow loading this state variable from a journal
    // 4. add constructors and [Serializable] attribute to all events
    // this code is located inside "factory.cs"
    #endregion

    #region Specifications, NUnit, and How To Start Test Runner Notes
    // then we start writing short "specification" stories, using C# syntax
    // the specs below are written as runnable NUnit tests and/or you can see similar
    // test output displayed to the console in this sample (Ctrl+F5).
    // Or...to see this in NUnit using Visual Studio instead, try right-clicking on the E007_re_factory project
    // and navigate to Test With --> NUnit and click "Run" in the NUnit test runner to see the test output
    #endregion

    #region Spec structure, Test Fixture, Unit Test Notes
    // when_receive_shipment_in_cargo_bay was the first Specification example mentioned in podcast Episode 5
    // typically one specification class like this is used per Aggregate command method
    // this class grouping of Specifications and Use Cases that test each method is also called a "Test Fixture"
    // each [Test] Specification covering a Use Case is also called a "Unit Test"
    // in this case, testing the behavior of the "ReceiveShipmentInCargoBay" method on the Factory Aggregate
    // and the serveral use cases related to that command method
    #endregion

    public sealed class when_assign_employee_to_factory : factory_specs
    {
        // **Employee Policies**

        [Test]
        public void empty_factory_allows_any_employee_not_bender_to_be_assigned()
        {
            // Given no events
            When = f => f.AssignEmployeeToFactory("fry");
            Then = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
        }

        [Test]
        public void duplicate_employee_name_is_assigned_but_not_allowed()
        {
            Given = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
            When = f => f.AssignEmployeeToFactory("fry");
            ThenException = ex => ex.Message.Contains("only one employee can have");
        }

        [Test]
        public void no_employee_named_bender_is_allowed_to_be_assigned()
        {
            // Given no Events
            When = f => f.AssignEmployeeToFactory("bender");
            ThenException = ex => ex.Message.Contains("Guys with name 'bender' are trouble");
        }
    }

    public sealed class when_receive_shipment_in_cargo_bay : factory_specs
    {
        // **Shipment Policies**

        [Test]
        public void a_shipment_received_announcement_is_made_with_correct_car_parts_list()
        {
            Given = new IEvent[]
                { 
                    new EmployeeAssignedToFactory("yoda")
                };
            When = f => f.ReceiveShipmentInCargoBay("shipment-777", new[] { new CarPart("engine", 1) });
            Then = new IEvent[]
                {
                    new ShipmentReceivedInCargoBay("shipment-777", new[] { new CarPart("engine", 1)})
                };
        }

        [Test]
        public void empty_shipment_is_not_allowed()
        {
            // notice the use of the "Given, When, Then" structure inside specifications

            Given = new IEvent[] { new EmployeeAssignedToFactory("yoda") };

            // When (we do something with a method, like receive a shipment in the cargo bay)
            When = f => f.ReceiveShipmentInCargoBay("some shipment", new CarPart[0]);

            // Then Expect these Events or These Exceptions
            ThenException = ex => ex.Message.Contains("Empty shipments are not accepted!");
        }

        [Test]
        public void an_empty_shipment_that_comes_to_factory_with_no_employees_is_not_received()
        {
            // Instead of some empty array of events like: Given = new IEvent[];
            // It is just left out, which does the same thing:  No Pre-Existing Events Yet!

            // So: Given NO EVENTS

            When = f => f.ReceiveShipmentInCargoBay("some shipment", new[] {new CarPart("chassis", 1)});
            ThenException = ex => ex.Message.Contains("has to be somebody at factory");
        }

        [Test]
        public void there_are_already_two_shipments_in_cargo_bay_so_no_new_shipments_allowed()
        {
            Given = new IEvent[]
                {
                    new EmployeeAssignedToFactory("chubakka"), 
                    new ShipmentReceivedInCargoBay("shipmt-11", new CarPart("engine",3)), 
                    new ShipmentReceivedInCargoBay("shipmt-12", new CarPart("wheels", 40)), 
                };
            When = f => f.ReceiveShipmentInCargoBay("shipmt-13", new CarPart("bmw6", 20));
            ThenException = ex => ex.Message.Contains("More than two shipments can't fit");
        }
    }

    public sealed class when_unpack_shipment_in_cargo_bay : factory_specs
    {
        // **Inventory Policies**

        [Test]
        public void an_unpacked_announcement_is_made_with_correct_inventory_list()
        {
            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("engine", 1);

            Given = new IEvent[]
                { 
                    new EmployeeAssignedToFactory("yoda"),
                    new ShipmentReceivedInCargoBay("shipment-777", new[] { new CarPart("engine", 1)})
                };
            When = f => f.UnpackAndInventoryShipmentInCargoBay("fet");
            Then = new IEvent[]
                {
                    new ShipmentUnpackedInCargoBay("fet", unpackedItems)
                };
        }

        [Test]
        public void it_is_an_error_if_there_are_no_shipments_to_unpack()
        {
            // Basically, event history has no record that a ShipmentReceivedInCargoBay has happened
            // Given = new IEvent[]; (there are no events)
            When = f => f.UnpackAndInventoryShipmentInCargoBay("darth");
            ThenException = ex => ex.Message.Contains("There are no shipments to unpack in this cargo bay");
        }

        [Test]
        public void an_employee_asked_to_unpack_more_than_once_a_day_is_not_allowed()
        {
            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("flux capacitor", 1);

            Given = new IEvent[]
                {
                    new ShipmentReceivedInCargoBay("shipment-99", new CarPart("flux capacitor", 1)),
                    new ShipmentUnpackedInCargoBay("yoda", unpackedItems),
                    new ShipmentReceivedInCargoBay("shipment-100", new CarPart("mcflywheel", 1)),
                };
            When = f => f.UnpackAndInventoryShipmentInCargoBay("yoda");
            ThenException = ex => ex.Message.Contains("has already unpacked a cargo bay today");
        }
    }

    public sealed class when_produce_a_car : factory_specs
    {
        // **Car Production Policies**

        [Test]
        public void cant_request_a_car_that_is_not_a_model_t()
        {
            // Given no Events
            When = f => f.ProduceACar("luke", "Model A");
            ThenException = ex => ex.Message.Contains("is not a car we can make.");
        }

        [Test]
        public void an_employee_who_has_already_produced_a_car_today_is_not_allowed()
        {
            Given = new IEvent[]
                {
                    new CarProduced("jabba", "Model T")
                };
            When = f => f.ProduceACar("jabba", "Model T");
            ThenException = ex => ex.Message.Contains("has already produced a car today, find someone else");
        }

        // Currrent Parts Needed To Build a Model T:
        // 6 wheels
        // 1 engine
        // 2 sets of "bits and pieces"

        [Test]
        public void not_enough_wheels_for_model_t_is_an_error()
        {
            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("wheels", 4);
            unpackedItems.Add("engine", 1);
            unpackedItems.Add("bits and pieces", 2);

            Given = new IEvent[]
                {
                    // Need 6 wheels for Model T
                    new ShipmentUnpackedInCargoBay("bob", unpackedItems)
                };

            When = f => f.ProduceACar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("Model T needs 6 wheels");

        }

        [Test]
        public void wheels_item_not_in_inventory_system_at_all_is_an_error()
        {
            var unpackedItems = new Dictionary<string, int>();
            //unpackedItems.Add("wheels", 4);
            unpackedItems.Add("engine", 1);
            unpackedItems.Add("bits and pieces", 2);

            Given = new IEvent[]
                {
                    // Need 6 wheels for Model T
                    new ShipmentUnpackedInCargoBay("bob", unpackedItems)
                };

            When = f => f.ProduceACar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("don't have any wheels in inventory");

        }
        
        [Test]
        public void not_enough_engines_for_model_t_is_an_error()
        {
            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("wheels", 6);
            unpackedItems.Add("engine", 0);
            unpackedItems.Add("bits and pieces", 2);

            Given = new IEvent[]
                {
                    // Need 1 engine for Model T
                    new ShipmentUnpackedInCargoBay("bob", unpackedItems)
                };
            When = f => f.ProduceACar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("Model T needs 1 engine");

        }

        [Test]
        public void not_enough_bits_and_pieces_for_model_t_is_an_error()
        {
            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("wheels", 6);
            unpackedItems.Add("engine", 1);
            unpackedItems.Add("bits and pieces", 1);

            Given = new IEvent[]
                {
                    // Need 2 bits and pieces for Model T
                    new ShipmentUnpackedInCargoBay("bob", unpackedItems)
                };
            When = f => f.ProduceACar("wicket", "Model T");
            ThenException = ex => ex.Message.Contains("Model T needs 2 bits and pieces");
        }

        [Test]
        public void having_available_employee_and_parts_for_model_t_results_in_a_car_produced()
        {
            // This is a SUCCESS test

            // Rule:  Parts Needed To Build a Model T
            // 6 wheels
            // 1 engine
            // 2 sets of "bits and pieces"

            var unpackedItems = new Dictionary<string, int>();
            unpackedItems.Add("wheels", 6);
            unpackedItems.Add("engine", 1);
            unpackedItems.Add("bits and pieces", 2);

            // TODO:  When I tried to add another car being produced by someone else first
            // TODO:  the test failed.  What did I forget to do or do wrong?

            Given = new IEvent[]
                {
                    //new EmployeeAssignedToFactory("luke"),
                    new EmployeeAssignedToFactory("chubakka"),
                    new ShipmentUnpackedInCargoBay("luke", unpackedItems),

                    // Another car has been produced by someone else today
                    //new CarProduced("luke", "Model T"),

                    // re-stock Inventory
                    //new ShipmentUnpackedInCargoBay("R2D2", unpackedItems)
                };
            When = f => f.ProduceACar("chubakka", "Model T");

            Then = new IEvent[]
                {
                    //new CarProduced("luke", "Model T"),
                    new CarProduced("chubakka", "Model T")
                };
        }
    }


    public class Program
    {
        public static void Main()
        {
            // you can run these tests from NUnit, but if you don't
            // have it, we'll run it in a console as well
            RunSpecification(new when_assign_employee_to_factory());
            RunSpecification(new when_receive_shipment_in_cargo_bay());
            // Homework
            RunSpecification(new when_unpack_shipment_in_cargo_bay());
            RunSpecification(new when_produce_a_car());
        }

        // Console Spec Runner Helpers
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
