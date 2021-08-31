using System;
using System.Collections.Generic;

#nullable disable

namespace EFCoreSamples.StabilityAndPerformance.Api.Persistence
{
    public partial class Customer
    {
        public Customer()
        {
            Sales = new HashSet<Sale>();
        }

        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<Sale> Sales { get; set; }
    }
}
