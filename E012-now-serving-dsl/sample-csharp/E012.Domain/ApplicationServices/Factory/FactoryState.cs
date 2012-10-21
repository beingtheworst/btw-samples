using System.Collections.Generic;
using System.Linq;
using E012.Contracts;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace E012.ApplicationServices.Factory
{
    /// <summary>
    /// This is the state of the Factory aggregate.
    /// It can be mutated only by passing events to it
    /// (either via constructor or mutation method).
    /// </summary>

    // TODO: In Episode 12 Rinat mentions that the FactoryState class should implement the IFactoryState interface to
    // ensure that we implement new Commands and Events as they are created or we will get build errors to remind us.
    // If I try this though:
    // public class FactoryState : IFactoryState
    // I get build errors about: "cannot implement an interface member because it is not public"
    // There are several suggested solutions out there like this:
    // http://blogs.msdn.com/b/johnwpowell/archive/2008/06/13/how-to-implement-an-interface-without-making-members-public-using-explicit-interface-implementation.aspx
    // But will wait for another time to resolve.  For now, will not claim to implement IFactoryState below.

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
        readonly Dictionary<string, int> _availableParts = new Dictionary<string, int>();

        public readonly List<string> ListOfEmployeeNames = new List<string>();
        public readonly Dictionary<string, InventoryShipment> ShipmentsWaitingToBeUnloaded = new Dictionary<string, InventoryShipment>();
        public readonly List<string> CreatedCars = new List<string>();

        public FactoryId Id { get; private set; }

        public int GetNumberOfAvailablePartsQuantity(string name)
        {
            return _availableParts.ContainsKey(name) ? _availableParts[name] : 0;
        }

        void When(FactoryOpened e)
        {
            Id = e.Id;
        }

        // announcements inside the factory
        void When(EmployeeAssignedToFactory e)
        {
            ListOfEmployeeNames.Add(e.EmployeeName);
        }
        void When(ShipmentTransferredToCargoBay e)
        {
            ShipmentsWaitingToBeUnloaded.Add(e.Shipment.Name, new InventoryShipment(e.Shipment.Name, e.Shipment.Cargo));
        }

        void When(CurseWordUttered e)
        {

        }

        void When(UnloadedFromCargoBay e)
        {
            foreach (var shipmentInCargoBay in e.InventoryShipments)
            {
                ShipmentsWaitingToBeUnloaded.Remove(shipmentInCargoBay.Name);

                foreach (var part in shipmentInCargoBay.Cargo)
                {
                    if (!_availableParts.ContainsKey(part.Name))
                        _availableParts.Add(part.Name, part.Quantity);
                    else
                        _availableParts[part.Name] += part.Quantity;
                }
            }
        }

        void When(CarProduced e)
        {
            CreatedCars.Add(e.CarModel);

            foreach (var carPart in e.Parts)
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
            // to call one of the "When" methods defined above.
            // The "dynamic" syntax below is just a shortcut we are using so we don't
            // have to create a large if/else block for a bunch of specific event types.
            // This shortcut "dynamic" syntax means:
            // "Call this FactoryState's instance of the When method
            // that has a method signature of:
            // When(WhateverTheCurrentTypeIsOfThe-e-EventThatWasPassedIntoMutate)".
            ((dynamic)this).When((dynamic)e);
        }


    }
}