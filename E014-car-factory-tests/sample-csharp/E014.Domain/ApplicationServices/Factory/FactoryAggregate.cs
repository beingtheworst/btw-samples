using System.Collections.Generic;
using System.Linq;
using E014.Contracts;

namespace E014.ApplicationServices.Factory
{
    /// <summary>
    /// <para>Implementation of the (car) factory aggregate.
    /// In production it is loaded and operated by a <see cref="FactoryApplicationService"/>, which loads it from
    /// event storage and calls the appropriate methods and passes arguments to them as needed.</para>
    /// <para>In test environments (thisEventTypeHappened.g. in unit tests), this aggregate can be instantiated directly
    /// or by wiring the same application service to the test environment.</para>
    /// </summary>
    public class FactoryAggregate
    {
        /// <summary> List of FactoryAggregate aggregateState changes represented by Events that have happened.</summary>
        public List<IEvent> EventsThatHappened = new List<IEvent>();

        /// <summary>
        /// AggregateState is kept separate from its Aggregate class in order to ensure
        /// that we modify an Aggregate's aggregateState ONLY by passing Events (event messages).
        /// </summary>
        readonly FactoryState _aggregateState;
        public FactoryAggregate(FactoryState aggregateState)
        {
            _aggregateState = aggregateState;
        }

        // Aggregate's method below modify internal "state" variables by doing their work and generating Events

        public void OpenFactory(FactoryId id)
        {
            // if the thing that tracks an aggregate's state already has and Id, the agg with that Id already exists!
            if (_aggregateState.Id != null)
                throw DomainError.Named("factory-already-created", "Factory was already created");

            RecordAndRealizeThat(new FactoryOpened(id));
        }


        public void AssignEmployeeToFactory(string employeeName)
        {
            ThrowExceptionIfFactoryIsNotOpen();

            if (_aggregateState.ListOfEmployeeNames.Contains(employeeName))
            {
                // yes, this is a really weird check, but this factory has really strict rules.
                // manager should've remembered that
                throw DomainError.Named("employee-name-already-taken", ":> the name of '{0}' only one employee can have", employeeName);
            }

            if (employeeName == "bender")
                throw DomainError.Named("bender-employee", ":> Guys with name 'bender' are trouble.");

            DoPaperWork("Assign employee to the factory");

            RecordAndRealizeThat(new EmployeeAssignedToFactory(_aggregateState.Id, employeeName));
        }


        public void ReceiveShipmentInCargoBay(string shipmentName, InventoryShipment shipment)
        {
            ThrowExceptionIfFactoryIsNotOpen();

            if (_aggregateState.ListOfEmployeeNames.Count == 0)
                throw DomainError.Named("unknown-employee", ":> There has to be somebody at factory in order to accept shipment");
            
            if (shipment.Cargo.Length == 0)
                throw DomainError.Named("empty-InventoryShipments", ":> Empty InventoryShipments are not accepted!");

            if (_aggregateState.ShipmentsWaitingToBeUnpacked.Count >= 2)
                throw DomainError.Named("more-than-two-InventoryShipments", ":> More than two InventoryShipments can't fit into this cargo bay :(");

            DoRealWork("opening cargo bay doors");

            RecordAndRealizeThat(new ShipmentReceivedInCargoBay(_aggregateState.Id, shipment));

            // TODO:  This is a requirement from before that I may need need to add back

            //var totalCountOfParts = shipment.Cargo.Sum(p => p.Quantity);
            //if (totalCountOfParts > 10)
            //{
            //    RecordAndRealizeThat(new CurseWordUttered(_aggregateState.Id, "Boltov tebe v korobky peredach",
            //                               "awe in the face of the amount of shipment delivered"));
            //}
        }

        public void UnpackAndInventoryShipmentInCargoBay(string employeeName)
        {
            ThrowExceptionIfFactoryIsNotOpen();

            if (_aggregateState.ListOfEmployeeNames.Count == 0)
                throw DomainError.Named("unknown-employee", ":> There has to be somebody at factory in order to accept shipment");

            if (!_aggregateState.ListOfEmployeeNames.Contains(employeeName))
                throw DomainError.Named("unknown-employee", ":> There has to be somebody at factory in order to accept shipment");
            

            // Rule: An Employee is only allowed to Unpack ONCE a Day
            if (_aggregateState.EmployeesWhoHaveUnpackedCargoBayToday.Contains(employeeName))
            {
                throw DomainError.Named("employee-already-unpacked-cargo", ":> '{0}' has already unpacked a cargo bay today", employeeName);
            }

            if (_aggregateState.ShipmentsWaitingToBeUnpacked.Count < 1)
            {
                throw DomainError.Named("empty-InventoryShipments", ":> InventoryShipments not found");
            }

            DoRealWork("'" + employeeName + "'" + " is unpacking the cargo bay");

            var shipments = new List<InventoryShipment>();

            while (_aggregateState.ShipmentsWaitingToBeUnpacked.Count > 0)
            {
                var parts = _aggregateState.ShipmentsWaitingToBeUnpacked.First();
                shipments.Add(parts.Value);

                RecordAndRealizeThat(new ShipmentUnpackedInCargoBay(_aggregateState.Id, employeeName, shipments.ToArray()));
            }

            // TODO: Rinat said this code line should be here instead of in while loop above but seems to cause lock-ups when moved here
            // re: https://github.com/abdullin/E014/commit/730393f578d9b2934ed767a9b7dcb27445c01ea9
            // TBD.  When left in while loop console test runner and NUnit does not lock-up.
            //RecordAndRealizeThat(new ShipmentUnpackedInCargoBay(_aggregateState.Id, employeeName, shipments.ToArray()));
        }

        // Note this method's dependency on the ICarBlueprintLibrary Domain Service as mentioned in BTW Podcast Episode 14
        public void ProduceACar(string employeeName, string carModel, ICarBlueprintLibrary carBlueprintLibrary)
        {
            ThrowExceptionIfFactoryIsNotOpen();

            if (!_aggregateState.ListOfEmployeeNames.Contains(employeeName))
                throw DomainError.Named("unknown-employee", ":> '{0}' not assigned to factory", employeeName);

            if (_aggregateState.EmployeesWhoHaveProducedACarToday.Contains(employeeName))
                throw DomainError.Named("employee-already-produced-car-today", ":> '{0}' not assigned to factory", employeeName);


            var design = carBlueprintLibrary.TryToGetBlueprintForModelOrNull(carModel);

            if (design == null)
                throw DomainError.Named("car-model-not-found", "Model '{0}' not found", carModel);


            var partsUsedToBuildCar = new List<CarPart>();

            foreach (var part in design.RequiredParts)
            {
                if (_aggregateState.GetNumberOfAvailablePartsQuantity(part.Name) < part.Quantity)
                    throw DomainError.Named("required-part-not-found", ":> {0} not found", part.Name);

                // remeber the CarPart that will be used to build the specififed carModel
                partsUsedToBuildCar.Add(new CarPart(part.Name, part.Quantity));
            }

            DoRealWork("produce a car - " +  "'" + employeeName + "'" + " is building a '" + carModel + "'");

            // As mentioned in Episode 12 of the BTW podcast, this code below is wrong.
            // The ICarBlueprintLibrary passed in, is not used.  Hard coded "parts" was ALWAYS used.
            // Tried to fix with the partsUsedToBuildCar approach but needs to be tested.
            // var parts = new[] { new CarPart("chassis", 1), new CarPart("wheels", 4), new CarPart("engine", 1) };

            RecordAndRealizeThat(new CarProduced(_aggregateState.Id, employeeName, carModel, partsUsedToBuildCar.ToArray()));
        }

        // Helpers Below

        void DoPaperWork(string workName)
        {
            //Print(" > Work:  papers... {0}... ", workName);
        }

        void DoRealWork(string workName)
        {
            //Print(" > Work:  heavy stuff... {0}...", workName);
        }

        void RecordAndRealizeThat(IEvent theEvent)
        {
            // we record by "writing down" the Event that happened in our "journal"

            EventsThatHappened.Add(theEvent);

            // and also immediately Realize theEvent has happened by changing _aggregateState after we have recorded it

            _aggregateState.MakeAggregateRealize(theEvent);
        }

        void ThrowExceptionIfFactoryIsNotOpen()
        {
            // in almost all cases a factory should be opened before
            // any other operation can proceed. We verify this here.
            if(_aggregateState.Id==null)
                throw DomainError.Named("factory-is-not-open", "Factory is not open");
        }
    }
}