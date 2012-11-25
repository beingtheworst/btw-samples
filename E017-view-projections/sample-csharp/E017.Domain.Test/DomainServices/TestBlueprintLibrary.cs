using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using E017.Contracts;
using E017.Domain.ApplicationServices;

namespace E017.Domain.DomainServices
{
    /// <summary>
    /// Test implementation of car blueprint domain service, which can be setup
    /// within the specifications
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestBlueprintLibrary : ICarBlueprintLibrary 
    {
        readonly IDictionary<string, CarPart[]> _knownDesigns = new Dictionary<string, CarPart[]>();

        public SpecSetupEvent RecordBlueprint(string name, params CarPart[] parts)
        {
            var description = string.Format("Registered car design '{0}' with following requirements:\r\n{1} ", name,
                parts.Aggregate("", (s, c) => string.Format("{0}\r\n\t{1}: {2}", s, c.Name, c.Quantity)));

            return new SpecSetupEvent(() => _knownDesigns.Add(name, parts), description);
        }

        CarBlueprint ICarBlueprintLibrary.TryToGetBlueprintForModelOrNull(string modelName)
        {
            return _knownDesigns.ContainsKey(modelName) ? new CarBlueprint(modelName, _knownDesigns[modelName]) : null;
        }
    }
}