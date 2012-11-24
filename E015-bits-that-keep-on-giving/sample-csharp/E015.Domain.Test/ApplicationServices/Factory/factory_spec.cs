using System;
using System.Collections.Generic;
using System.Linq;
using E015.ApplicationServices.Factory;
using E015.Contracts;
using E015.Domain.DomainServices;
// ReSharper disable InconsistentNaming
namespace E015.Domain.ApplicationServices.Factory
{
    /// <summary>
    /// Base class that acts as execution environment (container) for our application 
    /// service. It will be responsible for wiring in test version of services to
    /// factory and executing commands
    /// </summary>
    public abstract class factory_application_service_spec : application_service_spec
    {
        public TestBlueprintLibrary Library;

        protected override void SetupServices()
        {
            Library = new TestBlueprintLibrary();
        }

        protected override IEvent[] ExecuteCommand(IEvent[] given, ICommand cmd)
        {
            var store = new SingleCommitMemoryStore();
            foreach (var e in given.OfType<IFactoryEvent>())
            {
                store.Preload(e.Id.ToString(),e);
            }
            new FactoryApplicationService(store, Library).Execute(cmd);
            return store.Appended ?? new IEvent[0];
        }


        protected void When(IFactoryCommand when)
        {
            WhenMessage(when);
        }
        protected void Given(params IFactoryEvent[] given)
        {
            GivenMessages(given);
        }
        protected void GivenSetup(params SpecSetupEvent[] setup)
        {
            GivenMessages(setup);
        }
        protected void Expect(params IFactoryEvent[] given)
        {
            ExpectMessages(given);
        }

        // additional helper builders

        protected static InventoryShipment NewShipment(string name, params string[] partDescriptions)
        {
            return new InventoryShipment(name, NewCarPartList(partDescriptions));
        }

        protected static CarPart[] NewCarPartList(params string[] partDescriptions)
        {
            var parts = new List<CarPart>();

            foreach (var description in partDescriptions)
            {
                var items = description.Split(new char[] {' '}, 2);

                if (items.Length == 1)
                {
                    parts.Add(new CarPart(items[0], 1));
                }
                else
                {
                    parts.Add(new CarPart(items[1], int.Parse(items[0])));
                }
            }
            var carParts = parts.ToArray();
            return carParts;
        }
    }

    sealed class SingleCommitMemoryStore : IEventStore
    {
        public readonly IList<Tuple<string, IEvent>> Store = new List<Tuple<string, IEvent>>();
        public IEvent[] Appended = null; 
        public void Preload(string id, IEvent e)
        {
            Store.Add(Tuple.Create(id, e));
        }

        EventStream IEventStore.LoadEventStream(string id)
        {
            var events = Store.Where(i => id.Equals(i.Item1)).Select(i => i.Item2).ToList();
            return new EventStream
            {
                Events = events,
                StreamVersion = events.Count
            };
        }

        void IEventStore.AppendEventsToStream(string id, long expectedVersion, ICollection<IEvent> events)
        {
            if (Appended != null)
                throw new InvalidOperationException("Only one commit it allowed");
            Appended = events.ToArray();
        }
    }

}
