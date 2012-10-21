using System;
using E012.Contracts;

namespace E012.ApplicationServices.Factory
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
        private readonly ICarBlueprintLibrary _library;

        // pass dependencies that are needed for this application service via its constructor
        public FactoryApplicationService(IEventStore eventStore, ICarBlueprintLibrary library)
        {
            _eventStore = eventStore;
            _library = library;
        }

        // implementing this Execute method is a requirement of the IApplicationService interface
        public void Execute(object command)
        {
            // pass the command to a specific method named 'When'
            // that knows how to handle this type of command
            ((dynamic)this).When((dynamic)command);
        }

        // this Update method abstracts away the name of the exact aggregate method that we will be using/calling
        // this approach allows us to use this single Update method for multiple command messages
        // this method is where we implement the lifetime management of a FactoryAggregate in one place
        void Update(ICommand<FactoryId> c, Action<FactoryAggregate> execute)
        {
            // Load the event stream from the store using the FactoryId of the passed in command
            var eventStream = _eventStore.LoadEventStream(c.Id);

            // create a new Factory aggregate instance from its history of events
            var state = new FactoryState(eventStream.Events);
            var agg = new FactoryAggregate(state);

            // execute the delegated Action (lambda that contains a reference to a specific method call)
            // that was passed to this Update method by the "When" methods below
            execute(agg);

            // append resulting changes to the aggregate state to the event stream
            _eventStore.AppendEventsToStream(c.Id, eventStream.StreamVersion, agg.Changes);
        }

        // Now let's use the Update helper method above to wire command messages to actual factory aggregate methods
        public void When(ProduceCar c)
        {
            Update(c, ar => ar.ProduceCar(c.EmployeeName, c.CarModel, _library));
        }

        public void When(AssignEmployeeToFactory c)
        {
            Update(c, ar => ar.AssignEmployeeToFactory(c.EmployeeName));
        }

        public void When(CurseWordUttered c)
        {
            //throw new NotImplementedException();
        }

        public void When(TransferShipmentToCargoBay c)
        {
            Update(c,
                   ar =>
                   ar.TransferShipmentToCargoBay(c.ShipmentName, new InventoryShipment(c.ShipmentName, c.Parts)));
        }

        public void When(UnloadShipmentFromCargoBay c)
        {
            Update(c, ar => ar.UnloadShipmentFromCargoBay(c.EmployeeName));
        }

        public void When(OpenFactory c)
        {
            Update(c, ar => ar.OpenFactory(c.Id));
        }
    }
}