using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using E017;
using E017.ApplicationServices.Factory;
using E017.Contracts;
using E017.Projections;
using Microsoft.CSharp.RuntimeBinder;
using Platform;

namespace E017.Domain.Test.Console
{
    // This class is acting as sort of a mini-Inversion of Control (IoC) container for us.
    // One class that contains instances of our EventStore, FactoryApplication Service, car blueprint library, and a logger.
    public class ConsoleEnvironment
    {
        public IEventStore Events;
        public FactoryApplicationService FactoryAppService;
        public IDictionary<string, IShellAction> ActionHandlers;
        public InMemoryBlueprintLibrary Blueprints;
        public ILogger Log = LogManager.GetLoggerFor<ConsoleEnvironment>();
        public ActiveFactoriesProjection ActiveFactories;
        public WorkerRegistryProjection WorkerRegistry;
        public InventoryProjection Inventory;

        // public static method we call on startup to setup our simple console environment and to wire things up
        public static ConsoleEnvironment BuildEnvironment()
        {
            // a little bit of our simple infrastructure in here
            // this infrastructure binds our simple in-memory EventStore and multiple Projections to each other

            // E17 added the SynchronousEventHandler class to keep event handling simple without queues for now
            // this handler is now passed to our in-memory eventStore
            var eventHandlers = new SynchronousEventHandler();
            var eventStore = new InMemoryStore(eventHandlers);
            
            var blueprints = new InMemoryBlueprintLibrary();
            blueprints.Register("model-t", new CarPart("wheel",4), new CarPart("engine",1), new CarPart("chassis",1));

            // In non-demo environments we usually want Application Services decoupled from Projections
            // but we will not worry about that now for this simple example

            // Application Service (Command message handler)
            // get app service ready to respond to Command messages
            var factoryAppSvc = new FactoryApplicationService(eventStore, blueprints);


            // Projections (Event message handlers)
            // directly wire projections to the Event Handlers for now
            // (not using production-like queues to store the Events in for this simple example)

            // projections can also be thought of as almost another kind of Application Service
            // Application Services and Projections BOTH handle Messages, but
            // Projections just happen to subscribe & respond to "When..." Event messages ("eventHandlers"),
            // instead of "When..." Command messages

            var activeFactories = new ActiveFactoriesProjection();
            eventHandlers.RegisterHandler(activeFactories);

            var workerRegistry = new WorkerRegistryProjection();
            eventHandlers.RegisterHandler(workerRegistry);

            var inventory = new InventoryProjection();
            eventHandlers.RegisterHandler(inventory);

            // setup Window size values for Console Window that is 60% of Max Possible Size
            int winWidth = (System.Console.LargestWindowWidth * 6 / 10);
            int winHeight = (System.Console.LargestWindowHeight * 6 / 10);

            // hack - for now, hard code "bigger" buffer than Window sizes above
            // keep horizontal buffer equal to width - to avoid horizontal scrolling
            int winBuffWidth = winWidth;
            int winBuffHeight = winHeight + 300;
            System.Console.SetBufferSize(winBuffWidth, winBuffHeight);

            // Buffer is bigger than Window so set the Window Size
            System.Console.SetWindowSize(winWidth, winHeight);

            // note that various tricks to center Console Window on launch 
            // and to change to Font size were ugly (PInvoke, etc.) so left them out for now

            System.Console.Title = "Being The Worst Interactive Factory Shell";


            return new ConsoleEnvironment
                {
                    Events = eventStore,
                    FactoryAppService = factoryAppSvc,
                    ActionHandlers = ConsoleActions.Actions,
                    Blueprints = blueprints,
                    ActiveFactories = activeFactories,
                    WorkerRegistry = workerRegistry,
                    Inventory = inventory
                };
        }
    }

    public sealed class InMemoryStore : IEventStore
    {
        readonly ConcurrentDictionary<string, IList<IEvent>> _eventStore = new ConcurrentDictionary<string, IList<IEvent>>();
        readonly SynchronousEventHandler _eventHandler;

        static ILogger Log = LogManager.GetLoggerFor<InMemoryStore>();
        public InMemoryStore(SynchronousEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public EventStream LoadEventStream(string id)
        {
            var eventStream = _eventStore.GetOrAdd(id, new IEvent[0]).ToList();

            return new EventStream()
            {
                Events = eventStream,
                StreamVersion = eventStream.Count
            };
        }

        public void AppendEventsToStream(string id, long expectedVersion, ICollection<IEvent> events)
        {
            _eventStore.AddOrUpdate(id, events.ToList(), (s, list) => list.Concat(events).ToList());

            // to make the example simple, right after we "persist" the Events to the store above
            // we immediately call the projections that handle Events.

            foreach (var @event in events)
            {
                Log.Info("{0}", @event);

                // call the eventHandler so that all subscribed projections 
                // can "realize and react to" the Events that happend
                _eventHandler.Handle(@event);
            }
        }
    }

    public sealed class SynchronousEventHandler
    {
        readonly IList<object> _eventHandlers = new List<object>();
        public void Handle(IEvent @event)
        {
            foreach (var eventHandler in _eventHandlers)
            {
                // try to execute each Event that happend against
                // each eventHandler in the list and let it handle the Event if it cares or ignore it if it doesn't
                try
                {
                    ((dynamic)eventHandler).When((dynamic)@event);
                }
                catch (RuntimeBinderException e)
                {
                    // binding failure. Ignore
                }
            }
        }
        public void RegisterHandler(object projection)
        {
            _eventHandlers.Add(projection);
        }
    }

    public sealed class InMemoryBlueprintLibrary : ICarBlueprintLibrary
    {
        readonly IDictionary<string,CarBlueprint> _bluePrints = new Dictionary<string, CarBlueprint>(StringComparer.InvariantCultureIgnoreCase);

        static ILogger Log = LogManager.GetLoggerFor<InMemoryBlueprintLibrary>();
        public CarBlueprint TryToGetBlueprintForModelOrNull(string modelName)
        {
            CarBlueprint value;
            if (_bluePrints.TryGetValue(modelName,out value))
            {
                return value;
            }
            return null;
        }
        public void Register(string modelName, params CarPart[] parts)
        {
            Log.Debug("Adding new design {0}: {1}", modelName, string.Join(", ", parts.Select(p => string.Format("{0} x {1}", p.Quantity, p.Name))));
            _bluePrints[modelName] = new CarBlueprint(modelName, parts);
        }
    }
}