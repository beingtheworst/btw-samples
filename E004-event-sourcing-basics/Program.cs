#region (c) 2010-2012 Lokad All rights reserved

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;

namespace E003_event_sourcing_basics
{
    class Program
    {
        // let's talk about entities.
        // entity is something that has a name that is unique within a certain zone of interest
        // or this area has only one instance of this entity.

        // given name, we can always know how to find that entity (or a postal service will know that)
        // We say that entity is identified by name. in software that name is called identifier or identity

        // entity can change looks, behaviors, but it will still keep identifier. That's how we can find it.
        // or send it a message.

        // Now, let's talk about entities.

        // in previous lessons we've learned that message is a remote and decoupled equivalent
        // of a message call. 

        // So, now we will model entity called factory. We will write code that captures
        // actions it can be commanded to carry out. However, we will express these as method
        // calls instead of the commands. Names of method calls will be same as names of commands
        // parameters will match to the members.

        // it will be shorter right now (besides, you'll learn than message == method call),
        // besides, this approach will have additional meaning in real development down the road.


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

        // these methods will contain following elements (which can be really complex
        // or can be optional)
        // Checks (check if operation is allowed)
        // some work that might involve calculations, thinking, access to some tooling
        // Event that we write to journal to mark the work as being done.


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


            List<string> _ourListOfEmployeeNames = new List<string>();
            

            public void AssignEmployeeToFactory(string employeeName)
            {
                Print("?> Command: Assign employee {0} to factory", employeeName);
                // CheckIfEmployeeCanBeAssignedToFactory(employeeName);
                if (_ourListOfEmployeeNames.Contains(employeeName))
                {
                    // yes, this is really weird check, but this factory has really strict rules.
                    // manager should've remembered that
                    RecordThat(new EmployeeKickedOutOfFactory()
                        {
                            EmployeeName = employeeName,
                            Reason = string.Format("the name of '{0}' only one employee can have", employeeName)
                        });
                    return;
                }

                if (employeeName == "bender")
                {
                    RecordThat(new EmployeeKickedOutOfFactory()
                        {
                            EmployeeName = employeeName,
                            Reason = "Guys with name 'bender' are trouble."
                        });
                    return;
                }

                DoPaperWork("Assign employee to the factory");

                RecordThat(new EmployeeAssignedToFactory()
                    {
                        EmployeeName = employeeName
                    });
                // DoPaperWork();
                // RecordThatEmployeeAssignedToFactory(employeeName);
            }

            void DoPaperWork(string workName)
            {
                Print(" > Work:  papers... {0}... ", workName);
                Thread.Sleep(2000);
                
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





            void AnnounceInsideFactory(EmployeeAssignedToFactory e)
            {
                _ourListOfEmployeeNames.Add(e.EmployeeName);
            }
            void AnnounceInsideFactory(EmployeeKickedOutOfFactory e)
            {
                // this is a trick. Don't care about it for now
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
        public class EmployeeKickedOutOfFactory : IEvent
        {
            public string EmployeeName;
            public string Reason;

            public override string ToString()
            {
                return string.Format("'{0}' is not allowed, because '{1}'", EmployeeName, Reason);
            }
        }

        // let's run this implementation
        public static void RunFactoryImplementation3()
        {
            var factory = new FactoryImplementation3();
            factory.AssignEmployeeToFactory("yoda");
            factory.AssignEmployeeToFactory("luke");
            factory.AssignEmployeeToFactory("yoda");
            factory.AssignEmployeeToFactory("bender");
        }


        




        static void Main(string[] args)
        {
            // let's try running this
            RunFactoryImplementation3();
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
    }
}