using System;
using System.Collections.Generic;
using E017.Contracts;

namespace E017.Projections
{
    /// <summary>
    /// Simplified (for now) projection with in-memory storage
    /// </summary>
    public sealed class WorkerRegistryProjection
    {
        // using Tuple class shortcut here for this simple sample to avoid creating a Value Object
        // TUple is just a quick way to define a bag of stuff with specific property types.
        // TODO: Replace Tuple with Value Object that has EmployeeName and his/her Factory Id Association.

        public List<Tuple<string, FactoryId>> List = new List<Tuple<string, FactoryId>>();
  
        public void When(EmployeeAssignedToFactory e)
        {
            List.Add(Tuple.Create(e.EmployeeName, e.Id));
        }
    }
}