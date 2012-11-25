using System;
using E017.Contracts;

namespace E017.ApplicationServices.Factory
{
    /// <summary><para>
    /// This specific application service contains command handlers which load
    /// and operate <see cref="FactoryAggregate"/>. These handlers also pass the required
    /// dependencies to aggregate methods and perform conflict resolution. 
    /// </para><para>
    /// Command handlers are usually invoked by the infrastructure of an application
    /// server, which hosts the current service. Its infrastructure will be responsible
    /// for accepting message calls (in the form of web service calls or serialized
    /// command messages) and dispatching them to these command handlers.
    /// </para></summary>
    public sealed class FactoryApplicationService : IFactoryApplicationService, IApplicationService
    {
        // event store for accessing event streams
        private readonly IEventStore _eventStore;

        // domain service that is neeeded by the Factory aggregate
        private readonly ICarBlueprintLibrary _carBlueprintLibrary;

        // pass dependencies that are needed for this application service via its constructor
        public FactoryApplicationService(IEventStore eventStore, ICarBlueprintLibrary carBlueprintLibrary)
        {
            if (null == eventStore)
                throw new ArgumentNullException("eventStore");
            if (null == carBlueprintLibrary)
                throw new ArgumentNullException("carBlueprintLibrary");

            _eventStore = eventStore;
            _carBlueprintLibrary = carBlueprintLibrary;
        }

        // implementing this Execute method is a requirement of the IApplicationService interface
        public void Execute(object command)
        {
            // pass the command to a specific method named 'When'
            // that knows how to handle this type of command message

            ((dynamic)this).When((dynamic)command);
        }

        
        // this Update method abstracts away the name of the exact aggregate method that we will be using/calling
        // this approach allows us to use this single Update method for multiple command messages
        // this method is where we implement the lifetime management of an Aggregate in one place

        void Update(IFactoryCommand forAggregateIdentifiedBy, Action<FactoryAggregate> executeCommandUsingThis)
        {
            // Load the event stream from the event store using the FactoryId of the passed in command
            var key = forAggregateIdentifiedBy.Id.ToString();
            var eventStream = _eventStore.LoadEventStream(key);

            // create a new Factory aggregate instance from its history of allEventsRelatedToThisAggregateId
            var aggregateState = new FactoryState(eventStream.Events);
            var aggregate = new FactoryAggregate(aggregateState);

            // execute the delegated Action (lambda that contains a reference to a specific aggregate method call)
            // that was passed to this Update method by the "When" methods below
            executeCommandUsingThis(aggregate);

            // append resulting changes to the aggregate's event stream
            _eventStore.AppendEventsToStream(key, eventStream.StreamVersion, aggregate.EventsThatHappened);
        }

        // Now let's use the Update helper method above to wire command messages to actual aggregate methods
        public void When(ProduceACar cmd)
        {
            Update(cmd, ar => ar.ProduceACar(cmd.EmployeeName, cmd.CarModel, _carBlueprintLibrary));
        }

        public void When(AssignEmployeeToFactory cmd)
        {
            Update(cmd, ar => ar.AssignEmployeeToFactory(cmd.EmployeeName));
        }

        public void When(CurseWordUttered cmd)
        {
            //throw new NotImplementedException();
        }

        public void When(ReceiveShipmentInCargoBay cmd)
        {
            Update(cmd,
                   ar =>
                   ar.ReceiveShipmentInCargoBay(cmd.ShipmentName, new InventoryShipment(cmd.ShipmentName, cmd.CarParts)));
        }

        public void When(UnpackAndInventoryShipmentInCargoBay cmd)
        {
            Update(cmd, ar => ar.UnpackAndInventoryShipmentInCargoBay(cmd.EmployeeName));
        }

        public void When(OpenFactory cmd)
        {
            Update(cmd, ar => ar.OpenFactory(cmd.Id));
        }
    }
}