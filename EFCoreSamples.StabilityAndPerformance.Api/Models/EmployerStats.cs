namespace EFCoreSamples.StabilityAndPerformance.Api.Models;

public class EmployerStats
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int TotalSales { get; set; }
    public int TotalQuantity { get; set; }
    public IEnumerable<SaleModel> Sales { get; set; } 
}

public class SaleModel
{
    public int SaleId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}
