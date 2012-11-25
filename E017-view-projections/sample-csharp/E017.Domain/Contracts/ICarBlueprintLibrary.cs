namespace E017.Contracts
{
    /// <summary> <para>
    /// This is a sample of domain service that will be injected into an aggregate by an application service.
    /// During tests, this service will be replaced by a test implementation of the same 
    /// interface (no, you don't need mocking framework, just see the unit tests project). </para>
    /// </summary>
    public interface ICarBlueprintLibrary
    {
        CarBlueprint TryToGetBlueprintForModelOrNull(string modelName);
    }

    // This class defines the 'CarBlueprint' Value Object type that is returned by this interface
    public class CarBlueprint
    {
        public readonly string DesignName;
        public readonly CarPart[] RequiredParts;

        public CarBlueprint(string designName, CarPart[] requiredParts)
        {
            DesignName = designName;
            RequiredParts = requiredParts;
        }
    }

}