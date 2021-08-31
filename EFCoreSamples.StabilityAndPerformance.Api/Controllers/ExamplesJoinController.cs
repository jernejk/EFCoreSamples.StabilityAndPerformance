using EFCoreSamples.StabilityAndPerformance.Api.Models;
using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
using EFCoreSamples.StabilityAndPerformance.Api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSamples.StabilityAndPerformance.Api.Controllers
{
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
                    TotalSales = x.Sales.Count,
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

            EmployerStats result = new EmployerStats
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
    }
}
