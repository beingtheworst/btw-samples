using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace E011_specs_of_the_living_dead
{
    // the definition of the (Car) Factory itself
    public class FactoryAggregate
    {
        #region EventsThatHappened Naming Notes
        // In the Episode 4 (E004) sample code
        // we named the Journal variable "JournalOfFactoryEvents"
        // In E005 we changed it to its more broadly applicable production name of "Changes".
        // It is still the in memory list where we "write down" the EVENTS that HAVE HAPPENED.
        // In E007 I am torn with the wording/naming here.
        // "JournalOfFactoryEvents" is very specific to this Aggregate but in real world probably no Journal exists
        // "Changes" can be used generically across Aggregates but is not very descriptive to a domain expert
        // In the real world people would "see" or "hear" Events that they observe/sense
        // but I don't want "EventsIHeard" "EventsISaw" "EventsISensed" so I will go with generically reusable
        // name that people may actually say in real life.
        // For a balance between meaningful to domain and reusability I will try "EventsThatHappened" for now.
        #endregion

        public List<IEvent> EventsThatHappened = new List<IEvent>();

        # region The Place Where We Track FactoryState Has Moved To Its Own Class
        // Note that we have moved the place where we keep track of the current
        // state of the Factory.  In E004, Factory state was also inside of the Factory class itself.
        // Now, we have moved all Factory state into its own "FactoryState" class.
        # endregion

        readonly FactoryState _aggregateState;

        public FactoryAggregate(FactoryState aggregateState)
        {
            _aggregateState = aggregateState;
        }

        // methods that modify internal "state" variables by doing their work and generating Events
        public void AssignEmployeeToFactory(string employeeName)
        {
            //Print("?> Command: Assign employee {0} to factory", employeeName);

            if (_aggregateState.ListOfEmployeeNames.Contains(employeeName))
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

        public void ReceiveShipmentInCargoBay(string shipmentName, params CarPart[] parts)
        {
            //Print("?> Command: receive shipment in cargo bay");
            if (_aggregateState.ListOfEmployeeNames.Count == 0)
            {
                Fail(":> There has to be somebody at factory in order to accept shipment");
                return;
            }
            if (parts.Length == 0)
            {
                Fail(":> Empty shipments are not accepted!");
                return;
            }

            if (_aggregateState.ShipmentsWaitingToBeUnpacked.Count > 2)
            {
                Fail(":> More than two shipments can't fit into this cargo bay :(");
                return;
            }

            DoRealWork("opening cargo bay doors");
            RecordThat(new ShipmentReceivedInCargoBay(shipmentName, parts));

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

        public void UnpackAndInventoryShipmentInCargoBay(string employeeName)
        {
            //Print("?> Command: unpacking and inventorying shipment in cargo bay");

            // Rule: Are there actually shipments to unpack (cargo bay not empty)?
            if (_aggregateState.ShipmentsWaitingToBeUnpacked.Count < 1)
            {
                Fail(":> There are no shipments to unpack in this cargo bay :(");
                return;
            }

            // Rule: ONLY if the employee hasn't unpacked the cargo bay today
            if (_aggregateState.EmployeesWhoHaveUnpackedCargoBayToday.Contains(employeeName))
            {
                Fail(":> '" + employeeName + "' has already unpacked a cargo bay today, find someone else :");
                return;
            }

            DoRealWork("'" + employeeName + "'" + " is unpacking the cargo bay");

            // Unpack all Shipments in the CargoBay and Inventory them to the PartsShelf on Factory Floor.
            Dictionary<string, int> itemsUnpackedAndInventoried = new Dictionary<string,int>();

            // loop through all Shipments waiting in the Cargo Bay
            foreach (CarPart[] carParts in _aggregateState.ShipmentsWaitingToBeUnpacked)
            {
                // pull out each individual item in the shipment and add it to Inventory on the PartsShelf
                foreach (CarPart carPart in carParts)
                {
                    itemsUnpackedAndInventoried.Add(carPart.Name, carPart.Quantity);
                }
            }

            RecordThat(new ShipmentUnpackedInCargoBay(employeeName, itemsUnpackedAndInventoried));
        }

        public void ProduceACar(string employeeName, string carModel)
        {
            // Rule: Model T is the only car type that we can currently produce.
            if (carModel != "Model T")
            {
                Fail(":> '" + carModel + "' is not a car we can make. Can only make a 'Model T' :(");
                return;
            }

            // Rule: An Employee may only ProduceACar once a day.  Is one available to produce a car?
            if (_aggregateState.EmployeesWhoHaveProducedACarToday.Contains(employeeName))
            {
                Fail(":> '" + employeeName + "' has already produced a car today, find someone else :");
                return;
            }

            // Do we have enough parts to build a Model T?

            // Rule:  Parts Needed To Build a Model T
            // 6 wheels
            // 1 engine
            // 2 sets of "bits and pieces"

            int itemQuantity = 0;
            if (_aggregateState.InventoryOfPartsOnShelf.TryGetValue("wheels", out itemQuantity))
            {
                // wheels exist, do we have enough?
                if (itemQuantity < 6)
                {
                    Fail(":> Model T needs 6 wheels, we only have " + itemQuantity.ToString() + " :");
                    return;
                }
            }
            else
            {
                Fail(":> Model T needs 6 wheels, we don't have any wheels in inventory.  Did you forget to unpack a shipment? :");
                return;
            }

            itemQuantity = 0;
            if (_aggregateState.InventoryOfPartsOnShelf.TryGetValue("engine", out itemQuantity))
            {
                // engine exists, do we have enough?
                if (itemQuantity < 1)
                {
                    Fail(":> Model T needs 1 engine, we have " + itemQuantity.ToString() + " :");
                    return;
                }
            }
            else
            {
                Fail(":> Model T needs 1 engine, we don't have an engine in inventory.  Did you forget to unpack a shipment? :");
                return;
            }

            itemQuantity = 0;
            if (_aggregateState.InventoryOfPartsOnShelf.TryGetValue("bits and pieces", out itemQuantity))
            {
                // bits and pieces exist, do we have enough?
                if (itemQuantity < 2)
                {
                    Fail(":> Model T needs 2 bits and pieces, we only have " + itemQuantity.ToString() + " :");
                    return;
                }
            }
            else
            {
                Fail(":> Model T needs 2 bits and pieces, we don't have bits and pieces in inventory.  Did you forget to unpack a shipment? :");
                return;
            }

            
            DoRealWork("'" + employeeName + "'" + " is building a '" + carModel + "'");

            RecordThat(new CarProduced(employeeName, carModel));
        }

        // Helpers
        void DoPaperWork(string workName)
        {
            //Print(" > Work:  papers... {0}... ", workName);

        }
        void DoRealWork(string workName)
        {
            //Print(" > Work:  heavy stuff... {0}...", workName);

        }
        void RecordThat(IEvent theEvent)
        {
            // we record by jotting down notes of the Events that happened in our "journal"
            EventsThatHappened.Add(theEvent);
            // and also immediately change the state of the Aggregate after we officially record it
            _aggregateState.ChangeMyStateBecauseOf(theEvent);
        }
        void Fail(string message, params object[] args)
        {
            throw new InvalidOperationException(string.Format(message, args));
        }
    }


    #region The State of the Factory Has Moved To Its Own Class - Defined Here
    // FactoryState is a new class we added in this E005 sample to keep track of Factory state.
    // This moves the location of where Factory state is stored from the Factory class itself
    // to its own dedicated state class.  This is helpful because we can mark the
    // the state class properties as variables that cannot be modified outside of the FactoryState class.
    // (readonly, for example, is how we declared an instance of FactoryState at the top of this file)
    // (and the ListOfEmployeeNames and ShipmentsWaitingToBeUnpacked lists below are also declared as readonly)
    // This helps to ensure that you can ONLY MODIFY THE STATE OF THE FACTORY BY USING EVENTS that are known to have happened.
    #endregion

    public class FactoryState
    {
        public FactoryState(IEnumerable<IEvent> allEventsThatHaveEverHappened)
        {
            #region What This Constructor Is Doing
            // this will load and replay the "list" of all the events that are passed into this contructor
            // this brings this FactoryState instance up to date with 
            // all events that have EVER HAPPENED to its associated Factory aggregate entity
            // Note, I don't like funky @ symbols in my variables to bypass reserved words
            // and I aslo think the story reads better when it is called "eventThatHappened" anyway.
            #endregion
            foreach (var eventThatHappened in allEventsThatHaveEverHappened)
            {
                #region Naming Notes
                // call my public ChangeMyStateBecauseOf method (defined below) to get my current state up to date
                // This used to be called "Mutate" but in the code I saw that used it, renaming to
                // "ChangeMyStateBecauseOf" made the code story read better to me.
                #endregion
                ChangeMyStateBecauseOf(eventThatHappened);
            }
        }

        #region What Are These And Why readonly?
        // lock our state changes down to only Events that can modify these lists
        // these are the things that hold the data that represents 
        // our current understanding of the state of the factory
        // they get their data from the methods that use them while the methods react to Events that happen
        #endregion
        public readonly List<string> ListOfEmployeeNames = new List<string>();
        public readonly List<CarPart[]> ShipmentsWaitingToBeUnpacked = new List<CarPart[]>();
        public readonly List<string> ListOfCurseWordsUttered = new List<string>();
        public readonly List<string> EmployeesWhoHaveUnpackedCargoBayToday = new List<string>();
        public readonly Dictionary<string, int> InventoryOfPartsOnShelf = new Dictionary<string, int>();
        public readonly List<string> EmployeesWhoHaveProducedACarToday = new List<string>();

        #region How We Tell Everyone About Events That Have Happend With A Perfect Audit Log Of It
        // announcements inside the factory that get called by
        // the dynamic code shortcut in the ChangeMyStateBecauseOf method below.
        // As these methods change the content inside of the properties (lists inside) they call,
        // our understanding of the current state of the Factory is updated.
        // It is important to note that the official state of the Factory
        // that these methods change, only changes AFTER each Event they react to
        // has been RECORDED in the "journal" of "EventsThatHappened" defined at the start of this file.
        // If an Event hasn't been recorded in the EventsThatHappened list, the state
        // of the factory WILL NOT CHANGE.  State changes are ALWAYS reflected in the
        // stream of Events inside of the EventsThatHappened journal because these
        // "Announce" methods below are not executed until Events have been logged 
        // to the EventsThatHappened journal and have been called by the "RecordThat"'s call to "ChangeMyStateBecauseOf".
        // This is a very powerful aspect of Event Sourcing (ES).
        // We should NEVER directly modify these Aggregate state variables
        // (by calling the list directly for example), they are only ever modifed
        // as side effects of Events that have occured and have been logged.
        // This approach pretty much ensures a perfect audit log of all things that have ever happened.
        #endregion

        void AnnounceInsideFactory(EmployeeAssignedToFactory theEvent)
        {
            ListOfEmployeeNames.Add(theEvent.EmployeeName);
        }
        void AnnounceInsideFactory(ShipmentReceivedInCargoBay theEvent)
        {
            ShipmentsWaitingToBeUnpacked.Add(theEvent.CarParts);
        }
        void AnnounceInsideFactory(CurseWordUttered theEvent)
        {
            if (!ListOfCurseWordsUttered.Contains(theEvent.TheWord))
            {
                // if this word is not in the list, add it
                ListOfCurseWordsUttered.Add(theEvent.TheWord);
            }
        }
        void AnnounceInsideFactory(ShipmentUnpackedInCargoBay theEvent)
        {
            // The Factory uses a Dictionary<string, int> InventoryOfPartsOnShelf
            // to track all of its inventory of parts

            // loop through all inventoryItems that were unpacked and add to official inventory
            foreach (var inventoryItem in theEvent.ItemsUnpackedAndInventoried)
            {
                // See if this type of item is already in the Inventory System
                if (InventoryOfPartsOnShelf.ContainsKey(inventoryItem.Key))
                {
                    // this item type is already known in the inventory system, just change quantity
                    InventoryOfPartsOnShelf[inventoryItem.Key] =
                        InventoryOfPartsOnShelf[inventoryItem.Key] + inventoryItem.Value;
                }
                else
                {
                    // brand new Inventory Item, add it to the Inventory System
                    InventoryOfPartsOnShelf.Add(inventoryItem.Key, inventoryItem.Value);
                }
            }

            // Rule: All Shipments in the CargoBay are deemed unpacked and inventoried when this Event happens
            // So set the number of Shipments in the CargoBay to "0" by clearning the list.
            ShipmentsWaitingToBeUnpacked.Clear();

            // Rule: an Employee can only unpack all Shipments in the cargo bay once a day
            // so remember who just did it
            EmployeesWhoHaveUnpackedCargoBayToday.Add(theEvent.EmployeeName);
        }
        void AnnounceInsideFactory(CarProduced theEvent)
        {
            // Reduce the Inventory of parts that were just used to ProduceACar
            // There is a better way to not repeat the hard-coded Model T parts needed but leave it for now

            if (theEvent.CarModel == "Model T")
            {
                // Parts Needed To Build a Model T
                // 6 wheels
                // 1 engine
                // 2 sets of "bits and pieces
                // Reduce Parts Stock Inventory by that Amount

                int currentItemQuantity = 0;
                // TODO:  Rinat, This is atomic and safe right?  I think yes because of Factory Agg boundary but...??
                // TODO:  Negative inventory concerns, check on eventually consistent stuff.
                if (InventoryOfPartsOnShelf.TryGetValue("wheels", out currentItemQuantity))
                {
                        InventoryOfPartsOnShelf["wheels"] = currentItemQuantity - 6;
                }

                currentItemQuantity = 0;
                if (InventoryOfPartsOnShelf.TryGetValue("engine", out currentItemQuantity))
                {
                    InventoryOfPartsOnShelf["engine"] = currentItemQuantity - 1;
                }

                currentItemQuantity = 0;
                if (InventoryOfPartsOnShelf.TryGetValue("bits and pieces", out currentItemQuantity))
                {
                    InventoryOfPartsOnShelf["bits and pieces"] = currentItemQuantity - 2;
                }
            }

            // Rule: an employee can only produce one car per day
            // so remember who just did it

            EmployeesWhoHaveProducedACarToday.Add(theEvent.EmployeeName);
        }


        #region What Is This Method For And Why Is It Named This Way?
        // This is the very important "ChangeMyStateBecauseOf" method that provides the only public
        // way for Factory state to be modified.  "ChangeMyStateBecauseOf" ONLY ACCEPTS EVENTS that have happened.
        // It then CHANGES THE STATE of the Factory by calling the methods above
        // that wrap the readonly state variables that should be modified only when the associated Event(s)
        // that they care about have occured.
        // This method used to be called "Mutate" but in the code I saw that used it, renaming it to
        // "ChangeMyStateBecauseOf" made the code story read better to me.
        #endregion

        public void ChangeMyStateBecauseOf(IEvent theEvent)
        {
            #region What Is This Code Doing?
            // In addition to recording the Event, we also announce this Event inside of the Factory.
            // This way, all Factory Workers (people) will immediately know
            // what is going on inside the Factory.  We are telling the compiler
            // to call one of the "AnnounceInsideFactory" methods defined above.
            // The "dynamic" syntax below is just a shortcut we are using so we don't
            // have to create a large if/else block for a bunch of specific static Event types.
            // This shortcut using the "dynamic" keyword syntax means:
            // "Call this FactoryState's instance of the AnnounceInsideFactory method
            // that has a method signature of:
            // AnnounceInsideFactory(WhateverTheCurrentTypeIsOf-theEvent-ThatWasPassedIntoChangeMyStateBecauseOf)".
            #endregion

            ((dynamic)this).AnnounceInsideFactory((dynamic)theEvent);
        }
    }

    #region [Serializable] and [DataContract] Attribute Notes
    // notice that the "Serializable" attribute has been added above all Events in this sample
    // usually all Event implementation/contracts either have the Serializable (BinaryFormatter) or
    // DataContract (custom formatters) attribute above them (and any classes they call)
    // so they can be serialized for saving/communication
    #endregion

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
            return string.Format("new factory worker joins our forces: '{0}'", EmployeeName);
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
    public class ShipmentReceivedInCargoBay : IEvent
    {
        public string ShipmentName;
        public CarPart[] CarParts;

        public ShipmentReceivedInCargoBay(string shipmentName, params CarPart[] carParts)
        {
            ShipmentName = shipmentName;
            CarParts = carParts;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Shipment '{0}' received into cargo bay:", ShipmentName).AppendLine();
            foreach (var carPart in CarParts)
            {
                builder.AppendFormat("     {0} {1} pcs", carPart.Name, carPart.Quantity).AppendLine();
            }
            return builder.ToString();
        }
    }

    [Serializable]
    public class ShipmentUnpackedInCargoBay : IEvent
    {
        public string EmployeeName;
        public IDictionary<string, int> ItemsUnpackedAndInventoried;

        public ShipmentUnpackedInCargoBay(string employeeName, IDictionary<string, int> itemsUnpackedAndInventoried)
        {
            EmployeeName = employeeName;
            ItemsUnpackedAndInventoried = itemsUnpackedAndInventoried;
        }

        public override string ToString()
        {
            return string.Format("Employee: '{0}' unpacked and inventoried all shipments in the cargo bay", EmployeeName);
        }
    }

    [Serializable]
    public class CarProduced : IEvent
    {
        public string EmployeeName;
        public string CarModel;

        public CarProduced(string employeeName, string carModel)
        {
            EmployeeName = employeeName;
            CarModel = carModel;
        }

        public override string ToString()
        {
            return string.Format("Employee: '{0}' produced a '{1}'", EmployeeName, CarModel);
        }
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

    public interface IEvent
    {
        // I guess just a marker interface for now
    }

}