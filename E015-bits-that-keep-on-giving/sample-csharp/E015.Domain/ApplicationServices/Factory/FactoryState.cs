using System.Collections.Generic;
using System.Linq;
using E015.Contracts;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace E015.ApplicationServices.Factory
{
    /// <summary>
    /// This is the aggregateState of the Factory aggregate.
    /// It can be mutated/changed by passing in allEventsRelatedToThisAggregateId so you can make it realize this reality.
    /// (either via constructor or MakeAggregateRealize [mutation] method).
    /// </summary>

    // Note that in order to build the code when implemetning IFactoryState, we needed to make the "When" methods below public.
    public class FactoryState : IFactoryState
    {
        public FactoryState(IEnumerable<IEvent> allEventsRelatedToThisAggregateId)
        {
            #region What This Constructor Is Doing
            // this will load and replay the "list" of all the allEventsRelatedToThisAggregateId that are passed into this contructor
            // this brings this FactoryState instance up to date with 
            // all allEventsRelatedToThisAggregateId that have EVER HAPPENED to its associated Factory aggregate entity
            // Note, I don't like funky @ symbols in my variables to bypass reserved words
            // and I aslo think the story reads better when it is called "eventThatHappened" anyway.
            #endregion

            foreach (var eventThatHappened in allEventsRelatedToThisAggregateId)
            {
                #region Naming Notes
                // call my public MakeAggregateRealize method (defined below) to get my aggregateState up to date with all related Events
                // This used to be called "Mutate" but in the code I saw that used it, renaming to
                // "MakeAggregateRealize" made the code story read better to me.
                #endregion

                MakeAggregateRealize(eventThatHappened);
            }
        }

        // lock our aggregateState changes down to only allEventsRelatedToThisAggregateId that can modify these lists
        // this helps to ensure that we can ONLY MODIFY THE STATE OF THE AGGREGATE  BY USING EVENTS that are known to have happened.

        readonly Dictionary<string, int> _availableParts = new Dictionary<string, int>();

        public readonly List<string> ListOfEmployeeNames = new List<string>();
        public readonly Dictionary<string, InventoryShipment> ShipmentsWaitingToBeUnpacked = new Dictionary<string, InventoryShipment>();
        public readonly List<string> ListOfCurseWordsUttered = new List<string>();
        public readonly List<string> EmployeesWhoHaveUnpackedCargoBayToday = new List<string>();
        public readonly List<string> CreatedCars = new List<string>();
        public readonly List<string> EmployeesWhoHaveProducedACarToday = new List<string>();

        public FactoryId Id { get; private set; }

        public int GetNumberOfAvailablePartsQuantity(string name)
        {
            return _availableParts.ContainsKey(name) ? _availableParts[name] : 0;
        }

        // announcements inside the factory that we react to and realize WHEN thisEventTypeHappened happens
        public void When(FactoryOpened theEvent)
        {
            Id = theEvent.Id;
        }

        public void When(EmployeeAssignedToFactory theEvent)
        {
            ListOfEmployeeNames.Add(theEvent.EmployeeName);
        }

        public void When(ShipmentReceivedInCargoBay theEvent)
        {
            ShipmentsWaitingToBeUnpacked.Add(theEvent.Shipment.Name, 
                                            new InventoryShipment(theEvent.Shipment.Name,theEvent.Shipment.Cargo));
        }

        public void When(CurseWordUttered theEvent)
        {
            if (!ListOfCurseWordsUttered.Contains(theEvent.TheWord))
            {
                // if this word is not in the list, add it
                ListOfCurseWordsUttered.Add(theEvent.TheWord);
            }
        }

        public void When(ShipmentUnpackedInCargoBay theEvent)
        {
            foreach (var shipmentInCargoBay in theEvent.InventoryShipments)
            {
                ShipmentsWaitingToBeUnpacked.Remove(shipmentInCargoBay.Name);

                foreach (var part in shipmentInCargoBay.Cargo)
                {
                    if (!_availableParts.ContainsKey(part.Name))
                        _availableParts.Add(part.Name, part.Quantity);
                    else
                        _availableParts[part.Name] += part.Quantity;
                }
            }

            // Rule: an Employee can only unpack shipments in the cargo bay once a day
            // so remember who just did it
            EmployeesWhoHaveUnpackedCargoBayToday.Add(theEvent.EmployeeName);
        }

        public void When(CarProduced theEvent)
        {
            CreatedCars.Add(theEvent.CarModel);

            foreach (var carPart in theEvent.Parts)
            {
                var removed = carPart.Quantity;

                var quantitied = GetNumberOfAvailablePartsQuantity(carPart.Name);
                if (quantitied > 0)
                    _availableParts[carPart.Name] = quantitied > removed ? quantitied - removed : 0;
            }

            var emptyPartKeys = _availableParts.Where(x => x.Value == 0).Select(x => x.Key).ToList();

            foreach (var emptyPartKey in emptyPartKeys)
            {
                _availableParts.Remove(emptyPartKey);
            }

            // Rule: an employee can only produce one car per day
            // so remember who just did it

            EmployeesWhoHaveProducedACarToday.Add(theEvent.EmployeeName);

        }


        #region What Is This Method For And Why Is It Named This Way?
        // This is the very important "MakeAggregateRealize" method that provides the only public
        // way for Aggregate state to be modified.  "MakeAggregateRealize" ONLY ACCEPTS EVENTS that have happened.
        // It then CHANGES THE STATE of the Aggregate by calling the "When" methods above
        // that wrap the readonly aggregateState variables that should be modified only when the associated Event(s)
        // that they care about have occured.
        // This method used to be called "Mutate" but in the code I saw that used it, renaming it to
        // "MakeAggregateRealize" made the code story read better to me.
        #endregion

        public void MakeAggregateRealize(IEvent thisEventTypeHappened)
        {
           
            #region What Is This Code/Syntax Doing?
            // After the Aggregate records the Event, we announce this Event to all those
            // that care about it to help them Realize that things have changed.
            // We are telling the compiler to call one of the "When" methods defined above to achieve this realization.
            // The "dynamic" syntax below is just a shortcut we are using so we don't
            // have to create a large if/else block for a bunch of type-specific static Event types.
            // This shortcut using the "dynamic" keyword syntax means:
            // "Call 'this' Aggregates's instance of the 'When' method
            // that has a method signature of:
            // When(WhateverTheCurrentEventTypeIsOf-thisEventTypeHappened)".
            #endregion

            ((dynamic)this).When((dynamic)thisEventTypeHappened);
        }

    }
}