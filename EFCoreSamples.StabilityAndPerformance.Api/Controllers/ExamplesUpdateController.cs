using EFCoreSamples.StabilityAndPerformance.Api.Models;
using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSamples.StabilityAndPerformance.Api.Controllers;

/// <summary>
/// Testing pure performance and skipped on CancellationToken and Async.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ExamplesUpdateController : ControllerBase
{
    private readonly SalesDbContext _dbContext;

    public ExamplesUpdateController(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("worstCase")]
    public TestResult<int> WorstCase(bool isLoadFriendly = false)
    {
        IQueryable<Employee> query = GetBaseQuery(isLoadFriendly);
        var employees = query.ToList();
        foreach (var employee in employees)
        {
            string firstName = employee.FirstName;
            employee.FirstName = employee.LastName;
            employee.LastName = firstName;
        }

        _dbContext.SaveChangesAsync();

        return new TestResult<int>
        {
            Sql = query.ToQueryString() + "\n\nMany UPDATE SQL statements",
            LiveSql = false,
            Result = employees.Count
        };
    }

    /// <summary>
    /// Almost worst case scenario with no tracking.
    /// </summary>
    [HttpGet("updateQuery")]
    public TestResult<int> UpdateQuery(bool isLoadFriendly = false)
    {
        IQueryable<Employee> query = GetBaseQuery(isLoadFriendly);
        var employees = query
            .TagWith("Swap first and last name")
            .ExecuteUpdate(x => x
                .SetProperty(p => p.FirstName, b => b.LastName)
                .SetProperty(p => p.LastName, b => b.FirstName));

        _dbContext.SaveChangesAsync();

        return new TestResult<int>
        {
            Sql = query.ToQueryString() + "\n\nMany INSERT SQL statements",
            LiveSql = false,
            Result = employees
        };
    }

    private IQueryable<Employee> GetBaseQuery(bool isLoadFriendly = false)
        => !isLoadFriendly
        ? _dbContext.Employees
        : _dbContext.Employees.Take(1000);
}
