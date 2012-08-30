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
        // let's define our list of commands that the factory can carry out.
        public sealed class FactoryImplementation1
        {
            // the methods below are linguistically equivalent to a command message 
            // that could be sent to this factory. A command such as:
            // public class AssignEmployeeToFactory
            // {
            //    public string EmployeeName { get; set; }
            // }

            // in this sample we will not create command messages to represent 
            // and call these methods, we will just use the methods themselves to be our
            // "commands" for convenience.

            public void AssignEmployeeToFactory(string employeeName) {}
            public void TransferShipmentToCargoBay(string shipmentName, CarPart[] parts) {}
            public void UnloadShipmentFromCargoBay(string employeeName) {}
            public void ProduceCar(string employeeName, string carModel) {}
        }

        // these factory methods could contain the following elements (which can be 
        // really complex or can be optional):
        // * Checks (aka "guards") to see if an operation is allowed
        // * some work that might involve calculations, thinking, access to some tooling
        // * Events that we write to the journal to mark the work as being done.
        // These elements are noted as comments inside of the methods below for now
        public sealed class FactoryImplementation2
        {
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
                // DoPaperWork("Signing the shipment acceptance form");
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
            // Where all things that happen inside of the factory are recorded
            public List<IEvent> JournalOfFactoryEvents = new List<IEvent>();

            // internal "state" variables
            // these are the things that hold the data that represents 
            // our current understanding of the state of the factory
            // they get their data from the methods that use them while the methods react to events
            readonly List<string> _ourListOfEmployeeNames = new List<string>();
            readonly List<CarPart[]> _shipmentsWaitingToBeUnloaded = new List<CarPart[]>(); 

            public void AssignEmployeeToFactory(string employeeName)
            {
                Print("?> Command: Assign employee {0} to factory", employeeName);
                
                // Hey look, a business rule implementation!
                if (_ourListOfEmployeeNames.Contains(employeeName))
                {
                    // yes, this is really weird check, but this factory has really strict rules.
                    // manager should've remembered that!
                    Fail(":> the name of '{0}' only one employee can have", employeeName);
                    
                    return;
                }

                // another check that needs to happen when assigning employees to the factory
                // multiple options to prove this critical business rule:
                // John Bender: http://en.wikipedia.org/wiki/John_Bender_(character)#Main_characters
                // Bender Bending Rodríguez: http://en.wikipedia.org/wiki/Bender_(Futurama)
                if (employeeName == "bender")
                {
                    Fail(":> Guys with the name 'bender' are trouble.");
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


            // Remember that Factory Journal from above that is for writing
            // everything down?  Here is where we record stuff in it.
            void RecordThat(IEvent e)
            {
                JournalOfFactoryEvents.Add(e);

                // we also announce this event inside of the factory.
                // This way, all workers will immediately know
                // what is going on inside. In essence we are telling the compiler
                // to call one of the methods right below this"RecordThat" method.
                // The "dynamic" syntax below is a shortcut we are using so we don't
                // have to create a large if/else block for a bunch of specific event types.
                // "Call this factory's instance of the AnnounceInsideFactory method
                // that has a method signature of:
                // AnnounceInsideFactory(WhateverTheCurrentTypeIsOfThe-e-EventThatWasPassedIn)".

                ((dynamic) this).AnnounceInsideFactory((dynamic) e);

                // also print to console, just because we want to know
                Print("!> Event: {0}", e);
            }

            // announcements inside the factory that
            // get called by the dynamic code shortcut above.
            // As these methods change the content inside of the lists they call,
            // our understanding of the current state of the factory is updated.
            // It is important to note that the official state of the factory
            // that these methods change, only changes AFTER each event they react to
            // has been RECORDED in the journal.  If an event hasn't been recorded, the state
            // of the factory WILL NOT CHANGE.  State changes are ALWAYS reflected in the
            // stream of events inside of the journal because these methods are not
            // executed until events have been logged to the journal.
            // This is a very powerful aspect of event sourcing (ES).
            // We should NEVER directly modify the state variables
            // (by calling the list directly for example), they are only ever modifed
            // as side effects of events that have occured and have been logged.
            // Pretty much ensures a perfect audit log of what has happened.

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


        // let's run this implementation3 of the factory
        // (right-click on this project in Visual Studio and choose "set as StartUp project"
        // then push Ctrl+F5 to see this implementation of factory 3 running inside the console)

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
            // Hmm, a duplicate employee name, wonder if that will work?
            factory.AssignEmployeeToFactory("yoda");
            // An employee named "bender", why is that ringing a bell?
            factory.AssignEmployeeToFactory("bender");

            factory.TransferShipmentToCargoBay("model T spare parts", new[]
                {
                    new CarPart("wheels", 20),
                    new CarPart("engine", 7),
                    new CarPart("bits and pieces", 2)
                });


            Print("\r\nIt's the end of the day. Let's read our journal of events once more:\r\n");
            Print("\r\nWe should only see events below that were actually allowed to be recorded.\r\n");
            foreach (var e in factory.JournalOfFactoryEvents)
            {
                Print("!> {0}", e);
            }

            Print("\r\nIt seems, this was an interesting day!  Two Yoda's there should be not!");
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