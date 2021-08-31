namespace EFCoreSamples.StabilityAndPerformance.Api
{
    public class SqlRequest
    {
        public bool AsNoTracking { get; set; }
        public bool SelectOnlyIndex { get; set; }
        public bool Async { get; set; }
        public string Case { get; set; }
    }
}
