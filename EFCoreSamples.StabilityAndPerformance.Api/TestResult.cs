namespace EFCoreSamples.StabilityAndPerformance.Api;

public class TestResult
{
    public TestResult() { }

    public TestResult(string sql)
    {
        Sql = sql;
    }

    public string Sql { get; set; }
}

public class TestResult<T>
{
    public TestResult() { }

    public TestResult(string sql, T result)
    {
        Sql = sql;
    }

    public string Sql { get; set; }
    public bool LiveSql { get; set; }
    public T Result { get; set; }
}
