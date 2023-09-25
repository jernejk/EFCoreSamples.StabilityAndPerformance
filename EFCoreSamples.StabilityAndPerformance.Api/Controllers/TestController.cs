using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSamples.StabilityAndPerformance.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly SalesDbContext _dbContext;

    public TestController(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("where-count")]
    public int WhereCount()
    {
        var sales = _dbContext.Sales
            .Where(x => x.Quantity < 100)
            .ToList();

        return sales.Count;
    }

    [HttpGet("count")]
    public int Count()
    {
        return _dbContext.Sales.Count(x => x.Quantity < 100);
    }

    [HttpGet("countasync")]
    public async Task<int> CountAsync()
    {
        return await _dbContext.Sales.CountAsync(x => x.Quantity < 100);
    }

    [HttpGet("countasync-ct")]
    public async Task<int> AsyncCt(CancellationToken ct)
    {
        return await _dbContext.Sales.CountAsync(x => x.Quantity < 100, ct);
    }

    [HttpGet("tracking")]
    public int Tracking()
    {
        return _dbContext.Sales
            .Where(x => x.Quantity < 100)
            .ToList()
            .Count;
    }

    [HttpGet("notracking")]
    public int NoTracking()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.Quantity < 100)
            .ToList()
            .Count;
    }

    [HttpGet("notracking2")]
    public int NoTracking2()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.Quantity < 100)
            .Select(x => x.SalesId)
            .ToList()
            .Count;
    }

    [HttpGet("notracking3")]
    public int NoTracking3()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.Quantity < 100)
            .Select(x => new
            {
                x.ProductId,
                x.Quantity,
                x.SalesId,
                x.SalesPersonId,
                x.CustomerId
            })
            .ToList()
            .Count;
    }

    [HttpGet("join")]
    public int Join()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new
            {
                x.ProductId,
                x.Quantity,
                x.SalesId,
                x.SalesPersonId,
                SalesPerson = x.SalesPerson.FirstName,
            })
            .ToList()
            .Count;
    }

    [HttpGet("join2")]
    public int Join2()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.SalesPerson)
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new
            {
                x.ProductId,
                x.Quantity,
                x.SalesId,
                x.SalesPersonId,
                SalesPerson = x.SalesPerson.FirstName,
            })
            .ToList()
            .Count;
    }

    [HttpGet("join3")]
    public int Join3()
    {
        string salesPerson = _dbContext.Employees
            .AsNoTracking()
            .Where(x => x.EmployeeId == 1)
            .Select(x => x.FirstName)
            .First();
        return _dbContext.Sales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new
            {
                x.ProductId,
                x.Quantity,
                x.SalesId,
                x.SalesPersonId
            })
            .ToList()
            .Select(x => new
            {
                x.ProductId,
                x.Quantity,
                x.SalesId,
                x.SalesPersonId,
                SalesPerson = salesPerson,
            })
            .ToList()
            .Count;
    }

    [HttpGet("nosplit-join")]
    public void MultiJoin()
    {
        var result = _dbContext.Employees
            .AsNoTracking()
            .Include(x => x.Sales)
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.FirstName,
                x.LastName,
                SoldQuantity = x.Sales.Count,
                x.Sales
            })
            .FirstOrDefault();
    }

    [HttpGet("split-join")]
    public void MultiJoin2()
    {
        var result = _dbContext.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.FirstName,
                x.LastName,
                SoldQuantity = x.Sales.Count,
                x.Sales
            })
            .FirstOrDefault();
    }

    [HttpGet("split-join-manual")]
    public void MultiJoin3()
    {
        var salesPerson = _dbContext.Employees
            .AsNoTracking()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.EmployeeId,
                x.FirstName,
                x.LastName,
                SalesId = x.Sales.Select(x => x.SalesId)
            })
            .FirstOrDefault();

        var sales = _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == salesPerson.EmployeeId)
            .Select(x => new
            {
                x.SalesId,
                x.Quantity,
                x.ProductId,
                x.CustomerId
            })
            .FirstOrDefault();
    }

    [HttpGet("where-any-list")]
    public bool WhereAny()
    {
        var sales = _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == 1)
            .ToList();

        return sales.Any();
    }

    [HttpGet("where-any-first")]
    public bool WhereAnyFirst()
    {
        var sales = _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == 1)
            .FirstOrDefault();

        return sales != null;
    }

    [HttpGet("any")]
    public bool Any()
    {
        return _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == 1)
            .Any();
    }
}
