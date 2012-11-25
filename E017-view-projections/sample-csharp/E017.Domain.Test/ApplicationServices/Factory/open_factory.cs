using E017.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E017.Domain.ApplicationServices.Factory
{
    public class open_factory : factory_application_service_spec
    {
        readonly FactoryId Id = new FactoryId(42);

        [Test]
        public void open_factory_correclty_with_factory_id()
        {
            When(new OpenFactory(Id));
            Expect(new FactoryOpened(Id));
        }

        [Test]
        public void attempt_to_open_more_than_once_is_not_allowed()
        {
            Given(new FactoryOpened(Id));
            When(new OpenFactory(Id));
            ExpectError("factory-already-created");
        }
    }
}
