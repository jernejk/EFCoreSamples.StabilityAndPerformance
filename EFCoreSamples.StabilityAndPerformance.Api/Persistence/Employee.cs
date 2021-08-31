using System;
using System.Collections.Generic;

#nullable disable

namespace EFCoreSamples.StabilityAndPerformance.Api.Persistence
{
    public partial class Employee
    {
        public Employee()
        {
            Sales = new HashSet<Sale>();
        }

        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<Sale> Sales { get; set; }
    }
}
