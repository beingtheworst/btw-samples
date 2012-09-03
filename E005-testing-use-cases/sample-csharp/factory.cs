using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace E005_testing_use_cases
{
    public class FactoryAggregate
    {
        // THE Factory Journal!
        // In the Episode 4 (E004) sample code
        // we named the Journal variable "JournalOfFactoryEvents"
        // Here, we change it to its more broadly applicable production name of "Changes"
        // It is still the in memory list where we "write down" the EVENTS that HAVE HAPPENED.
        public List<IEvent> Changes = new List<IEvent>();

        // Note that we have moved the place where we keep track of the current
        // state of the Factory.  In E004, Factory state was also inside of the Factory class itself.
        // Now, we have moved all Factory state into its own "FactoryState" class.
        readonly FactoryState _state;

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

        public void TransferShipmentToCargoBay(string shipmentName, params CarPart[] parts)
        {
            //Print("?> Command: transfer shipment to cargo bay");
            if (_state.ListOfEmployeeNames.Count == 0)
            {
                Fail(":> There has to be somebody at factory in order to accept shipment");
                return;
            }
            if (parts.Length == 0)
            {
                Fail(":> Empty shipments are not accepted!");
                return;
            }

            if (_state.ShipmentsWaitingToBeUnloaded.Count > 2)
            {
                Fail(":> More than two shipments can't fit into this cargo bay :(");
                return;
            }

            DoRealWork("opening cargo bay doors");
            RecordThat(new ShipmentTransferredToCargoBay(shipmentName, parts));

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

    // FactoryState is a new class we added in this E005 sample to keep track of Factory state.
    // This moves the location of where Factory state is stored from the Factory class itself
    // to its own dedicated state class.  This is helpful because we can mark the
    // the state class properties as variables that cannot be modified outside of the FactoryState class.
    // (readonly, for example, is how we declared an instance of FactoryState at the top of this file)
    // (and the ListOfEmployeeNames and ShipmentsWaitingToBeUnloaded lists below are also declared as readonly)
    // This helps to ensure that you can ONLY MODIFY THE STATE OF THE FACTORY BY USING EVENTS that are known to have happened.

    public class FactoryState
    {
        public FactoryState(IEnumerable<IEvent> events)
        {
            // this will load and replay the "list" of all the events that are passed into this contructor
            // this brings this FactoryState instance up to date with 
            // all events that have EVER HAPPENED to its associated Factory aggregate entity
            foreach (var @event in events)
            {
                // call my public Mutate method (defined below) to get my state up to date
                Mutate(@event);
            }
        }

        // lock our state changes down to only events that can modify these lists
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

        // This is the very important Mutate method that provides the only public
        // way for factory state to be modified.  Mutate ONLY ACCEPTS EVENTS that have happened.
        // It then CHANGES THE STATE of the factory by calling the methods above
        // that wrap the readonly state variables that should be modified only when the associated event(s)
        // that they care about have occured.
        public void Mutate(IEvent e)
        {
            // we also announce this event inside of the factory.
            // this way, all workers will immediately know
            // what is going on inside the factory.  We are telling the compiler
            // to call one of the "AnnounceInsideFactory" methods defined above.
            // The "dynamic" syntax below is just a shortcut we are using so we don't
            // have to create a large if/else block for a bunch of specific event types.
            // This shortcut "dynamic" syntax means:
            // "Call this FactoryState's instance of the AnnounceInsideFactory method
            // that has a method signature of:
            // AnnounceInsideFactory(WhateverTheCurrentTypeIsOfThe-e-EventThatWasPassedIntoMutate)".


            ((dynamic)this).AnnounceInsideFactory((dynamic)e);
        }
    }


    // notice that the "Serializable" attribute has been added above all events in this sample
    // usually all event implementation/contracts either have the Serializable (BinaryFormatter) or
    // DataContract (custom formatters) attributes above them so they can be serialized for saving/communication
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

        public ShipmentTransferredToCargoBay(string shipmentName, params CarPart[] carParts)
        {
            ShipmentName = shipmentName;
            CarParts = carParts;
        }

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