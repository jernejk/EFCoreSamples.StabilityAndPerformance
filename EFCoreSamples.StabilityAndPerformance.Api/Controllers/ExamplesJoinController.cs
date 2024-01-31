using EFCoreSamples.StabilityAndPerformance.Api.Models;
using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EFCoreSamples.StabilityAndPerformance.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExamplesJoinController : ControllerBase
{
    private readonly SalesDbContext _dbContext;

    public ExamplesJoinController(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("worstCase")]
    public async Task<TestResult<int>> WorstCase(CancellationToken ct)
    {
        IQueryable<Sale> query = _dbContext.Sales
            .AsNoTracking()
            .TagWithContext()
            .Include(x => x.SalesPerson)
            .Include(x => x.Product)
            .Include(x => x.Customer)
            .Where(x => x.SalesPersonId == 1);

        List<Sale> dbResult = await query.TagWithContext().ToListAsync(ct);
        List<SalesWithSalesPerson> result = dbResult
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId,
                SalesPersonFirstName = x.SalesPerson.FirstName,
                SalesPersonLastName = x.SalesPerson.LastName
            })
            .ToList();

        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("badCase")]
    public async Task<TestResult<int>> BadCase(CancellationToken ct)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .TagWithContext()
            .Include(x => x.SalesPerson)
            .Where(x => x.SalesPersonId == 1);

        List<Sale> dbResult = await query.TagWithContext().ToListAsync(ct);
        List<SalesWithSalesPerson> result = dbResult
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId,
                SalesPersonFirstName = x.SalesPerson.FirstName,
                SalesPersonLastName = x.SalesPerson.LastName
            })
            .ToList();

        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("implicitJoin")]
    public async Task<TestResult<int>> ImplicitJoin(CancellationToken ct)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId,
                SalesPersonFirstName = x.SalesPerson.FirstName,
                SalesPersonLastName = x.SalesPerson.LastName
            });

        List<SalesWithSalesPerson> result = await query.TagWithContext().ToListAsync(ct);
        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("splitQuery")]
    public async Task<TestResult<int>> SplitQuery(CancellationToken ct)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId,
                SalesPersonFirstName = x.SalesPerson.FirstName,
                SalesPersonLastName = x.SalesPerson.LastName
            });

        List<SalesWithSalesPerson> result = await query.TagWithContext().ToListAsync(ct);
        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("splitQueryManualBad")]
    public async Task<TestResult<int>> SplitQueryManualBad(CancellationToken ct)
    {
        var salesPerson = _dbContext.Employees
            .AsNoTracking()
            .TagWithContext()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.FirstName,
                x.LastName
            })
            .First();

        var query = _dbContext.Sales
            .AsNoTracking()
            .TagWithContext()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId,
                SalesPersonFirstName = salesPerson.FirstName,
                SalesPersonLastName = salesPerson.LastName
            });

        List<SalesWithSalesPerson> result = await query.TagWithContext().ToListAsync(ct);
        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("splitQueryManual")]
    public async Task<TestResult<int>> SplitQueryManual(CancellationToken ct)
    {
        var salesPerson = _dbContext.Employees
            .AsNoTracking()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.FirstName,
                x.LastName
            })
            .First();

        var query = _dbContext.Sales
            .AsNoTracking()
            .TagWithContext()
            .Where(x => x.SalesPersonId == 1)
            .Select(x => new SalesWithSalesPerson
            {
                CustomerId = x.CustomerId,
                SalesId = x.SalesPersonId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                SalesPersonId = x.SalesPersonId
            });

        List<SalesWithSalesPerson> result = await query.TagWithContext().ToListAsync(ct);
        foreach (var sale in result)
        {
            sale.SalesPersonFirstName = salesPerson.FirstName;
            sale.SalesPersonLastName = salesPerson.LastName;
        }

        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.Count
        };
    }

    [HttpGet("complex")]
    public async Task<TestResult<int>> Complex(CancellationToken ct)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new EmployerStats
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                TotalSales = x.Sales.Count,
                //TotalQuantity = x.Sales.Sum(s => s.Quantity),
                Sales = x.Sales.Select(s => new SaleModel
                {
                    SaleId = s.SalesId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity
                })
            });

        EmployerStats result = await query.TagWithContext().FirstOrDefaultAsync(ct);
        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.TotalSales
        };
    }

    [HttpGet("complex-local-count")]
    public async Task<TestResult<int>> ComplexLocalCount(CancellationToken ct)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new EmployerStats
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                //TotalQuantity = x.Sales.Sum(s => s.Quantity),
                Sales = x.Sales.Select(s => new SaleModel
                {
                    SaleId = s.SalesId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity
                })
            });

        EmployerStats result = await query.TagWithContext().FirstOrDefaultAsync(ct);
        result.TotalSales = result.Sales.Count();
        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.TotalSales
        };
    }

    [HttpGet("complex-split-join")]
    public async Task<TestResult<int>> ComplexQueryJoin(CancellationToken ct)
    {
        // AsSplitQuery does not support Select inside a Select statement.
        var query = _dbContext.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .TagWithContext()
            .Include(x => x.Sales)
                .ThenInclude(x => x.Product)
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                //TotalQuantity = x.Sales.Sum(s => s.Quantity),
                Sales = x.Sales
            });


        var partialResult = await query.FirstOrDefaultAsync(ct);

        EmployerStats result = new EmployerStats
        {
            FirstName = partialResult.FirstName,
            LastName = partialResult.LastName,
            TotalSales = partialResult.Sales.Count,
            Sales = partialResult.Sales
                .Select(x => new SaleModel
                {
                    SaleId = x.SalesId,
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity
                })
        };

        return new TestResult<int>
        {
            Sql = query.ToQueryString(),
            LiveSql = true,
            Result = result.TotalSales
        };
    }

    [HttpGet("complex-split-join-manual")]
    public async Task<TestResult<int>> ComplexSplitQueryManual(CancellationToken ct)
    {
        var salesPerson = await _dbContext.Employees
            .AsNoTracking()
            .TagWithContext()
            .Where(x => x.EmployeeId == 1)
            .Select(x => new
            {
                x.EmployeeId,
                x.FirstName,
                x.LastName
            })
            .FirstOrDefaultAsync(ct);

        var sales = await _dbContext.Sales
            .AsNoTracking()
            .TagWithContext()
            .Where(x => x.SalesPersonId == salesPerson.EmployeeId)
            .Select(x => new
            {
                x.SalesId,
                x.Quantity,
                x.ProductId,
                x.CustomerId
            })
            .ToListAsync(ct);

        var productIds = sales
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();

        var products = await _dbContext.Products
            .AsNoTracking()
            .TagWithContext()
            .Where(x => productIds.Contains(x.ProductId))
            .Select(x => new
            {
                x.ProductId,
                x.Name
            })
            .ToListAsync(ct);

        var productLookup = products.ToDictionary(x => x.ProductId);

        EmployerStats result = new()
        {
            FirstName = salesPerson.FirstName,
            LastName = salesPerson.LastName,
            TotalSales = sales.Count,
            //TotalQuantity = sales.Sum(x => x.Quantity),
            Sales = sales.Select(x => new SaleModel
            {
                SaleId = x.SalesId,
                ProductName = productLookup[x.ProductId].Name,
                Quantity = x.Quantity
            })
        };

        return new TestResult<int>
        {
            Sql = "Lots of queries :)",
            LiveSql = true,
            Result = result.TotalSales
        };
    }

    /// <summary>
    /// Original query but with raw SQL.
    /// </summary>
    [HttpGet("rawSqlCommand")]
    public TestResult<int> RawSqlCommand()
    {
        string sql = @"SELECT [e].[FirstName], [e].[LastName], [s].[SalesId], [p].[Name], [s].[Quantity]
FROM [Employees] AS [e]
LEFT JOIN Sales as [s] on s.SalesPersonID = e.EmployeeID
LEFT JOIN Products as [p] on p.ProductID = s.ProductID
WHERE [e].[EmployeeID] = @EmployeeID
ORDER BY[s].SalesID";

        List<SaleModel> sales = new();
        EmployerStats employerStats = new();
        employerStats.Sales = sales;
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                employerStats.FirstName = result.GetString(0);
                employerStats.LastName = result.GetString(1);

                do
                {
                    sales.Add(new SaleModel
                    {
                        SaleId = result.GetInt32(2),
                        ProductName = result.GetString(3),
                        Quantity = result.GetInt32(4)
                    });
                }
                while (result.Read());
            }
        }

        return new TestResult<int>
        {
            Sql = sql,
            LiveSql = false,
            Result = employerStats.TotalSales
        };
    }

    /// <summary>
    /// Improved raw SQL.
    /// </summary>
    [HttpGet("rawSqlCommandSplit")]
    public TestResult<int> RawSqlCommandSplit()
    {
        string sql = @"SELECT [e].[FirstName], [e].[LastName]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @EmployeeID

SELECT [s].[SalesId], [p].[Name], [s].[Quantity]
FROM [Sales] AS [s]
LEFT JOIN Products as [p] on p.ProductID = s.ProductID
WHERE [s].[SalesPersonId] = @EmployeeID
ORDER BY[s].SalesID";

        List<SaleModel> sales = new();
        EmployerStats employerStats = new();
        employerStats.Sales = sales;
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();
            if (!result.Read())
            {
                return new TestResult<int>
                {
                    Sql = sql,
                    LiveSql = false,
                    Result = 0
                };
            }

            employerStats.FirstName = result.GetString(0);
            employerStats.LastName = result.GetString(1);

            result.NextResult();

            while (result.Read())
            {
                sales.Add(new SaleModel
                {
                    SaleId = result.GetInt32(0),
                    ProductName = result.GetString(1),
                    Quantity = result.GetInt32(2)
                });
            }
        }

        return new TestResult<int>
        {
            Sql = sql,
            LiveSql = false,
            Result = employerStats.TotalSales
        };
    }

    /// <summary>
    /// Attempting to optimize the raw SQL query #2.
    /// </summary>
    [HttpGet("rawSqlCommandSplitOnSql")]
    public TestResult<int> RawSqlCommandSplitOnSql()
    {
        // Yes, you can do all of that in a single query. :)
        string sql = @"DECLARE @productIDs TABLE (ProductID INT)

SELECT [e].[FirstName], [e].[LastName]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @EmployeeID

INSERT INTO @productIDs
SELECT [s].ProductID
FROM [Sales] AS [s]
WHERE [s].[SalesPersonId] = @EmployeeID
GROUP BY [s].ProductID

SELECT [p].[ProductID], [p].[Name]
FROM @productIDs as [pID]
LEFT JOIN Products as [p] on p.ProductID = [pID].[ProductID]

SELECT [s].[SalesId], [s].[ProductID], [s].[Quantity]
FROM [Sales] AS [s]
WHERE [s].[SalesPersonId] = @EmployeeID
ORDER BY[s].SalesID";

        List<SaleModel> sales = new();
        EmployerStats employerStats = new();
        employerStats.Sales = sales;
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();
            if (!result.Read())
            {
                return new TestResult<int>
                {
                    Sql = sql,
                    LiveSql = false,
                    Result = 0
                };
            }

            employerStats.FirstName = result.GetString(0);
            employerStats.LastName = result.GetString(1);

            result.NextResult();

            Dictionary<int, string> productNameLookup = new();
            while (result.Read())
            {
                productNameLookup.Add(result.GetInt32(0), result.GetString(1));
            }

            result.NextResult();

            while (result.Read())
            {
                int productId = result.GetInt32(1);
                sales.Add(new SaleModel
                {
                    SaleId = result.GetInt32(0),
                    ProductName = productNameLookup[productId],
                    Quantity = result.GetInt32(2)
                });
            }
        }

        employerStats.TotalSales = employerStats.Sales.Count();

        return new TestResult<int>
        {
            Sql = sql,
            LiveSql = false,
            Result = employerStats.TotalSales
        };
    }

    /// <summary>
    /// Attempting to optimize the raw SQL query #3.
    /// </summary>
    [HttpGet("rawSqlCommandSplitOptimized")]
    public TestResult<int> RawSqlCommandSplitOptimized()
    {
        // Yes, you can do all of that in a single query. :)
        string sql = @"DECLARE @productIDs TABLE (ProductID INT)
DECLARE @products TABLE (ProductID INT, Name NVARCHAR(50))

SELECT [e].[FirstName], [e].[LastName]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @EmployeeID

INSERT INTO @productIDs
SELECT [s].ProductID
FROM [Sales] AS [s]
WHERE [s].[SalesPersonId] = @EmployeeID
GROUP BY [s].ProductID

INSERT INTO @products
SELECT [p].[ProductID], [p].[Name]
FROM @productIDs as [pID]
LEFT JOIN Products as [p] on p.ProductID = [pID].[ProductID]

SELECT [s].[SalesId], [p].[Name], [s].[Quantity]
FROM [Sales] AS [s]
left join @products as [p] ON [p].ProductID = [s].ProductID
WHERE [s].[SalesPersonId] = @EmployeeID
ORDER BY[s].SalesID";

        List<SaleModel> sales = new();
        EmployerStats employerStats = new();
        employerStats.Sales = sales;
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();
            if (!result.Read())
            {
                return new TestResult<int>
                {
                    Sql = sql,
                    LiveSql = false,
                    Result = 0
                };
            }

            employerStats.FirstName = result.GetString(0);
            employerStats.LastName = result.GetString(1);

            result.NextResult();

            while (result.Read())
            {
                sales.Add(new SaleModel
                {
                    SaleId = result.GetInt32(0),
                    ProductName = result.GetString(1),
                    Quantity = result.GetInt32(2)
                });
            }
        }

        employerStats.TotalSales = employerStats.Sales.Count();

        return new TestResult<int>
        {
            Sql = sql,
            LiveSql = false,
            Result = employerStats.TotalSales
        };
    }

    /// <summary>
    /// Best performance so far (replicating EF Core optimized code).
    /// </summary>
    [HttpGet("rawSqlCommandWithManualSplit")]
    public TestResult<int> RawSqlCommandWithManualSplit()
    {
        string sql = @"SELECT [e].[FirstName], [e].[LastName]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @EmployeeID";

        EmployerStats employerStats = new();
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();
            if (!result.Read())
            {
                return new TestResult<int>
                {
                    Sql = sql,
                    LiveSql = false,
                    Result = 0
                };
            }

            employerStats.FirstName = result.GetString(0);
            employerStats.LastName = result.GetString(1);
        }

        sql = @"SELECT [s].[SalesId], [s].[ProductID], [s].[Quantity]
FROM [Sales] AS [s]
WHERE [s].[SalesPersonId] = @EmployeeID
ORDER BY[s].SalesID";

        List<Sale> sales = new();
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("EmployeeID", 1));
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();

            while (result.Read())
            {
                sales.Add(new Sale
                {
                    SalesId = result.GetInt32(0),
                    ProductId = result.GetInt32(1),
                    Quantity = result.GetInt32(2)
                });
            }
        }

        List<int> productIds = sales.Select(x => x.ProductId).Distinct().ToList();

        // NOTE: Tried to find a better way so that we don't get "SQL Injection" warning but this should be ok as an perf example. (also we dealing with ints directly)
        string condition = string.Join(",", productIds);
        sql = @$"SELECT [p].[ProductID], [p].[Name]
FROM [Products] AS [p]
WHERE [p].[ProductID] in ({condition})";

        Dictionary<int, string> productNameLookup = new();
        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            _dbContext.Database.OpenConnection();
            using DbDataReader result = command.ExecuteReader();

            while (result.Read())
            {
                productNameLookup.Add(result.GetInt32(0), result.GetString(1));
            }
        }

        employerStats.Sales = sales.Select(x => new SaleModel
        {
            SaleId = x.SalesId,
            ProductName = productNameLookup[x.ProductId],
            Quantity = x.Quantity
        });

        employerStats.TotalSales = employerStats.Sales.Count();

        return new TestResult<int>
        {
            Sql = sql,
            LiveSql = false,
            Result = employerStats.TotalSales
        };
    }
}
