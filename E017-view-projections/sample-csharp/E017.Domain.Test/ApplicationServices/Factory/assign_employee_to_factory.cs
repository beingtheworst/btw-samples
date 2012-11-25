using E017.Contracts;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E017.Domain.ApplicationServices.Factory
{
    public class assign_employee_to_factory : factory_application_service_spec
    {
        static readonly FactoryId Id = new FactoryId(11);

        [Test]
        public void empty_factory_allows_any_employee_not_bender_to_be_assigned()
        {
            Given(new FactoryOpened(Id));
            When(new AssignEmployeeToFactory(Id, "fry"));
            Expect(new EmployeeAssignedToFactory(Id, "fry"));
        }

        [Test]
        public void duplicate_employee_name_is_assigned_but_not_allowed()
        {
            Given(new FactoryOpened(Id),
                    new EmployeeAssignedToFactory(Id,"fry"));
            When(new AssignEmployeeToFactory(Id, "fry"));
            ExpectError("employee-name-already-taken");
        }
        [Test]
        public void no_employee_named_bender_is_allowed_to_be_assigned()
        {
            Given(new FactoryOpened(Id));
            When(new AssignEmployeeToFactory(Id, "bender"));
            ExpectError("bender-employee");
        }

        [Test]
        public void cant_assign_employee_to_unopened_factory()
        {
            When(new AssignEmployeeToFactory(Id, "fry"));
            ExpectError("factory-is-not-open");
        }
    }
}
