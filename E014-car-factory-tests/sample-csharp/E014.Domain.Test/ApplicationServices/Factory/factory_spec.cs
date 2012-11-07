using System.Collections.Generic;
using E014.ApplicationServices.Factory;
using E014.Contracts;
using E014.Domain.DomainServices;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices.Factory
{
    /// <summary>
    /// Base class that acts as execution environment (container) for our application 
    /// service. It will be responsible for wiring in test version of services to
    /// factory and executing commands
    /// </summary>
    public abstract class factory_application_service_spec : application_service_spec<FactoryId>
    {
        public TestBlueprintLibrary<FactoryId> Library;

        protected override void SetupServices()
        {
            Library = new TestBlueprintLibrary<FactoryId>();
        }

        protected override void ExecuteCommand(IEventStore store, ICommand<FactoryId> cmd)
        {
            new FactoryApplicationService(store,Library).Execute(cmd);
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
}
