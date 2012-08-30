using System;
using System.Reflection;
using NUnit.Framework;

namespace E005_testing_use_cases
{

    // first we refactor our factory from previous episode a little bit.
    // 1. rename TheFactoryJournal into -> Changes
    // 2. move state variables into a separate class (so that we can't accidentally touch it)
    // 3. allow loading this state variable from a journal
    // 4. add constructors and [Serializable] attribute to all events
    // this code is located inside "factory.cs"
    
    // then we start writing short stories, using C# syntax

    public sealed class when_assign_employee_to_factory : factory_specs
    {
        [Test]
        public void given_empty_factory()
        {
            When = f => f.AssignEmployeeToFactory("fry");
            Then = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
        }
        [Test]
        public void given_bob_assigned_to_factory()
        {
            Given = new IEvent[]
                {
                    new EmployeeAssignedToFactory("fry")
                };
            When = f => f.AssignEmployeeToFactory("fry");
            ThenException = ex => ex.Message.Contains("only one employee can have");
        }
        [Test]
        public void given_empty_factory_and_bender()
        {
            When = f => f.AssignEmployeeToFactory("bender");
            ThenException = ex => ex.Message.Contains("Guys with name 'bender' are trouble");
        }
    }


    public class Program
    {
        public static void Main()
        {
            // you can run these tests from NUnit, but if you don't
            // have it, we'll run it in a console as well
            RunSpecification(new when_assign_employee_to_factory());
        }

        static void RunSpecification(factory_specs specification)
        {
            Console.WriteLine(new string('=', 80));
            var cases = specification.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Console.WriteLine("Specification {0}", specification.GetType().Name);
            foreach (var methodInfo in cases)
            {
                Console.WriteLine(new string('-', 80));
                Console.WriteLine("Use case: {0}", methodInfo.Name.Replace("_"," "));
                Console.WriteLine();
                try
                {
                    specification.Setup();
                    methodInfo.Invoke(specification, null);
                    Console.WriteLine("PASSED!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("FAIL!");
                }
            }
        }
    }

    


    

}
