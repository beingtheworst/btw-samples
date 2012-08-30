#region (c) 2010-2012 Lokad All rights reserved

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace E004_event_sourcing_basics
{
    class Program
    {
        // let's define our list of commands that factory can carry out.
        public sealed class FactoryImplementation1
        {
            // this is linguistically equivalent to command that is sent to this factory
            // public class AssignEmployeeToFactory
            // {
            //    public string EmployeeName { get; set; }
            // }
            public void AssignEmployeeToFactory(string employeeName) {}
            public void TransferShipmentToCargoBay(string shipmentName, CarPart[] parts) {}
            public void UnloadShipmentFromCargoBay(string employeeName) {}
            public void ProduceCar(string employeeName, string carModel) {}
        }

        // these factory methods will contain following elements (which can be 
        // really complex or can be optional):
        // * Checks (check if operation is allowed)
        // * some work that might involve calculations, thinking, access to some tooling
        // * Events that we write to journal to mark the work as being done.
        public sealed class FactoryImplementation2
        {
            // this is linguistically equi
            public void AssignEmployeeToFactory(string employeeName)
            {
                // CheckIfEmployeeCanBeAssignedToFactory(employeeName);
                // DoPaperWork();
                // RecordThatEmployeeAssignedToFactory(employeeName);
            }

            public void TransferShipmentToCargoBay(string shipmentName, CarPart[] parts)
            {
                // CheckIfCargoBayHasFreeSpace(parts);
                // DoRealWork("unloading supplies...");
                // DoPaperWork("Signing that shipment acceptance form");
                // RecordThatSuppliesAreAvailableInCargoBay()
            }

            public void UnloadShipmentFromCargoBay(string employeeName)
            {
                // DoRealWork("passing supplies");
                // RecordThatSuppliesWereUnloadedFromCargoBay()
            }

            public void ProduceCar(string employeeName, string carModel)
            {
                // CheckIfWeHaveEnoughSpareParts
                // CheckIfEmployeeIsAvailable
                // DoRealWork
                // RecordThatCarWasProduced
            }
        }


        // Now let's "unwrap" AssignEmployeeToFactory
        // we'll start by adding a list of employees

        public class FactoryImplementation3
        {
            // THE Factory Journal!
            public List<IEvent> JournalOfFactoryEvents = new List<IEvent>();

            // internal "state" variables
            readonly List<string> _ourListOfEmployeeNames = new List<string>();
            readonly List<CarPart[]> _shipmentsWaitingToBeUnloaded = new List<CarPart[]>(); 

            public void AssignEmployeeToFactory(string employeeName)
            {
                Print("?> Command: Assign employee {0} to factory", employeeName);
                
                if (_ourListOfEmployeeNames.Contains(employeeName))
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
                RecordThat(new EmployeeAssignedToFactory
                    {
                        EmployeeName = employeeName
                    });
            }

            public void TransferShipmentToCargoBay(string shipmentName, CarPart[] parts)
            {
                Print("?> Command: transfer shipment to cargo bay");
                if (_ourListOfEmployeeNames.Count == 0)
                {
                    Fail(":> There has to be somebody at factory in order to accept shipment");
                    return;
                }

                if (_shipmentsWaitingToBeUnloaded.Count> 2)
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
                Print(" > Work:  papers... {0}... ", workName);
                Thread.Sleep(1000);
            }
            void DoRealWork(string workName)
            {
                Print(" > Work:  heavy stuff... {0}...", workName);
                Thread.Sleep(1000);
            }
            void RecordThat(IEvent e)
            {
                // we record by jotting down notes in our journal
                JournalOfFactoryEvents.Add(e);
                // we also announce this event inside factory.
                // so that all workers will immediately know
                // what is going inside
                ((dynamic) this).AnnounceInsideFactory((dynamic) e);

                // ok, we also print to console, just because we want to know
                Print("!> Event: {0}", e);
            }

            // announcements inside the factory
            void AnnounceInsideFactory(EmployeeAssignedToFactory e)
            {
                _ourListOfEmployeeNames.Add(e.EmployeeName);
            }
            void AnnounceInsideFactory(ShipmentTransferredToCargoBay e)
            {
                _shipmentsWaitingToBeUnloaded.Add(e.CarParts);
            }
            void AnnounceInsideFactory(CurseWordUttered e)
            {
                
            }
        }

        public class EmployeeAssignedToFactory : IEvent
        {
            public string EmployeeName;

            public override string ToString()
            {
                return string.Format("new worker joins our forces: '{0}'", EmployeeName);
            }
        }

        public class CurseWordUttered : IEvent
        {
            public string TheWord;
            public string Meaning;

            public override string ToString()
            {
                return string.Format("'{0}' was heard within the walls. It meant:\r\n    '{1}'", TheWord, Meaning);
            }
        }
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


        // let's run this implementation

        static void Main(string[] args)
        {
            Print("A new day at the factory starts...\r\n");
            var factory = new FactoryImplementation3();
            

            factory.TransferShipmentToCargoBay("chassis", new[]
                {
                    new CarPart("chassis", 4), 
                });

            factory.AssignEmployeeToFactory("yoda");
            factory.AssignEmployeeToFactory("luke");
            factory.AssignEmployeeToFactory("yoda");
            factory.AssignEmployeeToFactory("bender");

            factory.TransferShipmentToCargoBay("model T spare parts", new[]
                {
                    new CarPart("wheels", 20),
                    new CarPart("engine", 7),
                    new CarPart("bits and pieces", 2)
                });


            Print("\r\nIt's the end of the day. Let's read our journal once more:\r\n");
            foreach (var e in factory.JournalOfFactoryEvents)
            {
                Print("!> {0}", e);
            }

            Print("\r\nIt seems, this was an interesting day!");
        }

        static void Print(string format, params object[] args)
        {
            if (format.StartsWith("!"))
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            else if (format.StartsWith("?"))
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine();
            }
            else if (format.StartsWith(" >"))
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(format, args);
        }

        static void Fail(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(format, args);
        }
    }

    public interface IEvent
    {
        
    }

    public sealed class Factory
    {
        public void AssignEmployeeToFactory(string name) {}

        public void ShipSuppliesToCargoBay(string shipment, CarPart[] parts) {}

        public void UnloadSuppliesFromCargoBay(string employee) {}

        public void ProduceCar(string employee, string carModel) {}
    }

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