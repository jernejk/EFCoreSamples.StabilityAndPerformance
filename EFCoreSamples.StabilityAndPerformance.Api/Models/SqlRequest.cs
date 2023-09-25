namespace EFCoreSamples.StabilityAndPerformance.Api.Models;

public class SqlRequest
{
    public bool AsNoTracking { get; set; }
    public bool SelectOnlyIndex { get; set; }
    public bool Async { get; set; }
    public string Case { get; set; }
}
