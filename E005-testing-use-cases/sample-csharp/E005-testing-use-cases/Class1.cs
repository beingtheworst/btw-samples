using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace E005_testing_use_cases
{
    public class Program
    {
        // first we refactor our factory from previous episode a little bit.
        // 1. rename TheFactoryJournal into -> Changes
        // 2. move state variables into a separate class (so that we can't accidentally touch it)
        // 3. allow loading this state variable from a journal
        // 4. add constructors and [Serializable] attribute to all events
        

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

    
    public sealed class when_assign_employee_to_factory : factory_specs
    {
        [Test]
        public void given_empty_factory()
        {
            When = f => f.AssignEmployeeToFactory("fry");
            Then = new List<IEvent>
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


    public class FactoryAggregate
    {
        // THE Factory Journal!
        public List<IEvent> Changes = new List<IEvent>();
        FactoryState _state;

        public FactoryAggregate(FactoryState state)
        {
            _state = state;
        }

        // internal "state" variables
        
        public void AssignEmployeeToFactory(string employeeName)
        {
            //Print("?> Command: Assign employee {0} to factory", employeeName);

            if (_state.ListOfEmployeeNames.Contains(employeeName))
            {
                // yes, this is really weird check, but this factory has really strict rules.
                // manager should've remembered that
                Fail(":> the name of '{0}' only one employee can have", employeeName);

                return;
            }

            if (employeeName == "bender")
            {
                Fail(":> Guys with name 'bender' are trouble.");
                return;
            }

            DoPaperWork("Assign employee to the factory");
            RecordThat(new EmployeeAssignedToFactory(employeeName));
        }

        void Fail(string message, params object[] args)
        {
            throw new InvalidOperationException(string.Format(message, args));
        }

        public void TransferShipmentToCargoBay(string shipmentName, CarPart[] parts)
        {
            //Print("?> Command: transfer shipment to cargo bay");
            if (_state.ListOfEmployeeNames.Count == 0)
            {
                Fail(":> There has to be somebody at factory in order to accept shipment");
                return;
            }

            if (_state.ShipmentsWaitingToBeUnloaded.Count > 2)
            {
                Fail(":> More than two shipments can't fit into this cargo bay :(");
                return;
            }

            DoRealWork("opening cargo bay doors");
            RecordThat(new ShipmentTransferredToCargoBay()
            {
                ShipmentName = shipmentName,
                CarParts = parts
            });

            var totalCountOfParts = parts.Sum(p => p.Quantity);
            if (totalCountOfParts > 10)
            {
                RecordThat(new CurseWordUttered
                {
                    TheWord = "Boltov tebe v korobky peredach",
                    Meaning = "awe in the face of the amount of parts delivered"
                });
            }
        }


        void DoPaperWork(string workName)
        {
            //Print(" > Work:  papers... {0}... ", workName);
            
        }
        void DoRealWork(string workName)
        {
            //Print(" > Work:  heavy stuff... {0}...", workName);
            
        }
        void RecordThat(IEvent e)
        {
            // we record by jotting down notes in our journal
            Changes.Add(e);
            // and also immediately change the state
            _state.Mutate(e);
        }

      
    }

    public class FactoryState
    {
        public FactoryState(IEnumerable<IEvent> events)
        {
            // this will load all events 
            foreach (var @event in events)
            {
                Mutate(@event);
            }
        }

        public readonly List<string> ListOfEmployeeNames = new List<string>();
        public readonly List<CarPart[]> ShipmentsWaitingToBeUnloaded = new List<CarPart[]>();

        // announcements inside the factory
        void AnnounceInsideFactory(EmployeeAssignedToFactory e)
        {
            ListOfEmployeeNames.Add(e.EmployeeName);
        }
        void AnnounceInsideFactory(ShipmentTransferredToCargoBay e)
        {
            ShipmentsWaitingToBeUnloaded.Add(e.CarParts);
        }
        void AnnounceInsideFactory(CurseWordUttered e)
        {

        }
        public void Mutate(IEvent e)
        {
            // we also announce this event inside factory.
            // so that all workers will immediately know
            // what is going inside. In essence we are telling compiler
            // to call one of the methods below
            ((dynamic)this).AnnounceInsideFactory((dynamic)e);
        }
    }

    [Serializable]
    public class EmployeeAssignedToFactory : IEvent
    {
        public string EmployeeName;

        public EmployeeAssignedToFactory(string employeeName)
        {
            EmployeeName = employeeName;
        }

        public override string ToString()
        {
            return string.Format("new worker joins our forces: '{0}'", EmployeeName);
        }
    }
    [Serializable]
    public class CurseWordUttered : IEvent
    {
        public string TheWord;
        public string Meaning;

        public override string ToString()
        {
            return string.Format("'{0}' was heard within the walls. It meant:\r\n    '{1}'", TheWord, Meaning);
        }
    }
    [Serializable]
    public class ShipmentTransferredToCargoBay : IEvent
    {
        public string ShipmentName;
        public CarPart[] CarParts;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Shipment '{0}' transferred to cargo bay:", ShipmentName).AppendLine();
            foreach (var carPart in CarParts)
            {
                builder.AppendFormat("     {0} {1} pcs", carPart.Name, carPart.Quantity).AppendLine();
            }
            return builder.ToString();
        }
    }

    public interface IEvent
    {

    }

    [Serializable]
    public sealed class CarPart
    {
        public string Name;
        public int Quantity;
        public CarPart(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }

}
