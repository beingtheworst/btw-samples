using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using E015;
using E015.ApplicationServices.Factory;
using E015.Contracts;
using Platform;

namespace E015.Btw.Portable
{
    // This class is acting as sort of a mini-Inversion of Control (IoC) container for us.
    // One class that contains instances of oour EventStore, FactoryApplication Service, car blueprint library, and a logger.
    public class ConsoleEnvironment
    {
        public IEventStore Events;
        public FactoryApplicationService FactoryAppService;
        public IDictionary<string, IShellAction> Handlers;
        public InMemoryBlueprintLibrary Blueprints;
        public ILogger Log = LogManager.GetLoggerFor<ConsoleEnvironment>();

        public static ConsoleEnvironment BuildEnvironment()
        {
            var store = new InMemoryStore();
            

            var blueprints = new InMemoryBlueprintLibrary();
            blueprints.Register("model-t", new CarPart("wheel",4), new CarPart("engine",1), new CarPart("chassis",1));
            var fas = new FactoryApplicationService(store, blueprints);
            return new ConsoleEnvironment
                {
                    Events = store,
                    FactoryAppService = fas,
                    Handlers = ConsoleActions.Actions,
                    Blueprints = blueprints
                };
        }
    }

    public sealed class InMemoryStore : IEventStore
    {
        readonly ConcurrentDictionary<string, IList<IEvent>> _store = new ConcurrentDictionary<string, IList<IEvent>>();

        static ILogger Log = LogManager.GetLoggerFor<InMemoryStore>();
        public EventStream LoadEventStream(string id)
        {
            var stream = _store.GetOrAdd(id, new IEvent[0]).ToList();

            return new EventStream()
            {
                Events = stream,
                StreamVersion = stream.Count
            };
        }

        public void AppendEventsToStream(string id, long expectedVersion, ICollection<IEvent> events)
        {
            foreach (var @event in events)
            {
                Log.Info("{0}", @event);
            }
            _store.AddOrUpdate(id, events.ToList(), (s, list) => list.Concat(events).ToList());
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