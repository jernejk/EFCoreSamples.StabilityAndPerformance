using EFCoreSamples.StabilityAndPerformance.Api.Models;
using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
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
    public class ExamplesPaginationsController : Controller
    {
        private readonly SalesDbContext _dbContext;

        private const int DefaultPageIndex = 10;
        private const int DefaultPageSize = 20;
        private const int DefaultSalesPersonId = 1;

        public ExamplesPaginationsController(SalesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("worstCase")]
        public async Task<TestResult<int>> WorstCase(int salesPersonId = DefaultSalesPersonId, int page = DefaultPageIndex, int pageSize = DefaultPageSize, CancellationToken ct = default)
        {
            IQueryable<Sale> query = _dbContext.Sales
                .AsNoTracking()
                // Very commonly used in combination of in-memory pagination, making things even worse
                .Include(x => x.SalesPerson)
                .Where(x => x.SalesPersonId == salesPersonId);

            List<Sale> dbResult = await query.ToListAsync(ct);
            List<SalesWithSalesPerson> result = dbResult
                .Skip(page * pageSize)
                .Take(pageSize)
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
                Result = dbResult.Count
            };
        }

        [HttpGet("executedOnDB")]
        public async Task<TestResult<int>> ExecutedOnDb(int salesPersonId = DefaultSalesPersonId, int page = DefaultPageIndex, int pageSize = DefaultPageSize, CancellationToken ct = default)
        {
            IQueryable<SalesWithSalesPerson> query = _dbContext.Sales
                .AsNoTracking()
                .Where(x => x.SalesPersonId == salesPersonId)
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

            // After all conditions are applied, count them in DB.
            int count = await query.CountAsync(ct);

            // Apply paginations, sorts and more complex select statements.
            query = query
                .Skip(page * pageSize)
                .Take(pageSize);

            List<SalesWithSalesPerson> result = await query.ToListAsync(ct);

            return new TestResult<int>
            {
                Sql = query.ToQueryString(),
                LiveSql = true,
                Result = count
            };
        }

        [HttpGet("executedOnDBSplitQuery")]
        public async Task<TestResult<int>> ExecutedOnDbSplitQuery(int salesPersonId = DefaultSalesPersonId, int page = DefaultPageIndex, int pageSize = DefaultPageSize, CancellationToken ct = default)
        {
            IQueryable<SalesWithSalesPerson> query = _dbContext.Sales
                .AsNoTracking()
                .Where(x => x.SalesPersonId == salesPersonId)
                .Select(x => new SalesWithSalesPerson
                {
                    CustomerId = x.CustomerId,
                    SalesId = x.SalesPersonId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    SalesPersonId = x.SalesPersonId
                });

            // After all conditions are applied, count them in DB.
            int count = await query.CountAsync(ct);

            // Apply paginations, sorts and more complex select statements.
            query = query
                .Skip(page * pageSize)
                .Take(pageSize);

            List<SalesWithSalesPerson> result = await query.ToListAsync(ct);

            var salePersonIds = result.Select(x => x.SalesPersonId).Distinct().ToArray();
            var salePersons = await _dbContext.Employees
                .Where(x => salePersonIds.Contains(x.EmployeeId))
                .ToListAsync(ct);

            var employeeLookup = salePersons.ToDictionary(x => x.EmployeeId);
            foreach (var sale in result)
            {
                var employee = employeeLookup[sale.SalesPersonId];
                sale.SalesPersonFirstName = employee.FirstName;
                sale.SalesPersonLastName = employee.LastName;
            }

            return new TestResult<int>
            {
                Sql = query.ToQueryString(),
                LiveSql = true,
                Result = count
            };
        }

        [HttpGet("bonus")]
        public async Task<TestResult<int>> Bonus(int? salesPersonId, int? customerId, int? productId, bool includeProduct = false, int page = DefaultPageIndex, int pageSize = DefaultPageSize, CancellationToken ct = default)
        {
            IQueryable<Sale> salesQuery = _dbContext.Sales
                .AsNoTracking();

            if (salesPersonId.HasValue)
            {
                salesQuery = salesQuery.Where(x => x.SalesPersonId == salesPersonId);
            }

            if (customerId.HasValue)
            {
                salesQuery = salesQuery.Where(x => x.CustomerId ==  customerId);
            }

            if (productId.HasValue)
            {
                salesQuery = salesQuery.Where(x => x.ProductId == productId);
            }

            // After all conditions are applied, count them in DB.
            int count = await salesQuery.CountAsync(ct);

            IQueryable<SalesDetails> saleDetailsQuery;
            if (includeProduct)
            {
                saleDetailsQuery = salesQuery
                    .Select(x => new SalesDetails
                    {
                        CustomerId = x.CustomerId,
                        SalesId = x.SalesPersonId,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        SalesPersonId = x.SalesPersonId,
                        SalesPersonFirstName = x.SalesPerson.FirstName,
                        SalesPersonLastName = x.SalesPerson.LastName,
                        Product = new ProductModel
                        {
                            ProductId = x.ProductId,
                            Name = x.Product.Name
                        }
                    });
            }
            else
            {
                saleDetailsQuery = salesQuery
                    .Select(x => new SalesDetails
                    {
                        CustomerId = x.CustomerId,
                        SalesId = x.SalesPersonId,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        SalesPersonId = x.SalesPersonId,
                        SalesPersonFirstName = x.SalesPerson.FirstName,
                        SalesPersonLastName = x.SalesPerson.LastName
                    });
            }

            saleDetailsQuery = saleDetailsQuery
                .Skip(page * pageSize)
                .Take(pageSize);

            // Apply paginations, sorts and more complex select statements.
            List<SalesDetails> result = await saleDetailsQuery.ToListAsync(ct);

            return new TestResult<int>
            {
                Sql = saleDetailsQuery.ToQueryString(),
                LiveSql = true,
                Result = count
            };
        }
    }
}
