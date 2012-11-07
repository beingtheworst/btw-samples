using System.Collections.Generic;
using System.Linq;
using E014.Contracts;
using E014.Domain.ApplicationServices;

namespace E014.Domain.DomainServices
{
    /// <summary>
    /// Test implementation of car blueprint domain service, which can be setup
    /// within the specifications to be passed into Aggregates that depend on it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestBlueprintLibrary<T> : ICarBlueprintLibrary where T : IIdentity
    {
        readonly IDictionary<string, CarPart[]> _knownDesigns = new Dictionary<string, CarPart[]>();

        // this method returns an Event that allows the test framework to react to it and add a known car design to blueprint library
        public IEvent<T> RecordBlueprint(string name, params CarPart[] parts)
        {
            var description = string.Format("Registered car design '{0}' with following requirements:\r\n{1} ", name,
                parts.Aggregate("", (s, c) => string.Format("{0}\r\n\t{1}: {2}", s, c.Name, c.Quantity)));

            return new SpecSetupEvent<T>(() => _knownDesigns.Add(name, parts), description);
        }

        CarBlueprint ICarBlueprintLibrary.TryToGetBlueprintForModelOrNull(string modelName)
        {
            return _knownDesigns.ContainsKey(modelName) ? new CarBlueprint(modelName, _knownDesigns[modelName]) : null;
        }
    }
}