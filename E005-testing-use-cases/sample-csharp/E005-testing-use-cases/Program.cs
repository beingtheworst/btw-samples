using System;
using System.Reflection;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E005_testing_use_cases
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


    public sealed class when_assign_employee_to_factory : factory_specs
    {
        [Test]
        public void empty_factory()
        {
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

    public sealed class when_transfer_shipment_to_cargo_bay : factory_specs
    {
        [Test]
        public void empty_shipment()
        {
            Given = new IEvent[] { new EmployeeAssignedToFactory("yoda")  };
            When = f => f.TransferShipmentToCargoBay("some shipment", new CarPart[0]);
            ThenException = ex => ex.Message.Contains("Empty shipments are not accepted!");
        }
        [Test]
        public void empty_shipment_comes_to_empty_factory()
        {
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


    public class Program
    {
        public static void Main()
        {
            // you can run these tests from NUnit, but if you don't
            // have it, we'll run it in a console as well
            RunSpecification(new when_assign_employee_to_factory());
            RunSpecification(new when_transfer_shipment_to_cargo_bay());
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
                    Print(ConsoleColor.DarkRed, "\r\nFAIL!");
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
