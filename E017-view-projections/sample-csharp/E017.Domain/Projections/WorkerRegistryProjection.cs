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
        public List<Tuple<string, FactoryId>> List = new List<Tuple<string, FactoryId>>();
  
        public void When(EmployeeAssignedToFactory e)
        {
            List.Add(Tuple.Create(e.EmployeeName, e.Id));
        }
    }
}