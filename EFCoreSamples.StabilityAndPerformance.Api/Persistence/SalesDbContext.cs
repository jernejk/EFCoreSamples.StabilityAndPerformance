using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace EFCoreSamples.StabilityAndPerformance.Api.Persistence;

public sealed class ActualExecutionPlanInterceptor : DbCommandInterceptor
{
    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
    {
        if (result.CommandText.Contains("-- IncludeExecutionPlan"))
        {
            // Inject actual execution plan.
            result.CommandText = $@"SET STATISTICS XML ON; -- OnInit

{result.CommandText}";
        }

        return base.CommandInitialized(eventData, result);
    }

    public override InterceptionResult DataReaderClosing(DbCommand command, DataReaderClosingEventData eventData, InterceptionResult result)
    {
        // Assume that if it's not closed, has at least a result set with at least 1 row, it's most likely execution plan.
        if (!eventData.DataReader.IsClosed && eventData.DataReader.NextResult() && eventData.DataReader.Read())
        {
            string executionPlan = eventData.DataReader.GetString(0);
            if (eventData.Context is IActualExecutionPlanDbContext dbContext)
            {
                dbContext.LastActualExecutionPlan = executionPlan;
            }

        }

        return base.DataReaderClosing(command, eventData, result);
    }
}

public interface IActualExecutionPlanDbContext
{
    string? LastActualExecutionPlan { get; set; }
}

public static class Ext
{
    public static async Task<(List<TSource> Result, string? ExecutionPlan)> ToListWithPlanAsync<TSource>(
        this IQueryable<TSource> source, IActualExecutionPlanDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        List<TSource> result = await source
            .TagWith("IncludeExecutionPlan")
            .ToListAsync(cancellationToken);

        return (Result: result, ExecutionPlan: dbContext.LastActualExecutionPlan);
    }
}

public partial class SalesDbContext(DbContextOptions<SalesDbContext> options)
    : DbContext(options), IActualExecutionPlanDbContext
{
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Sale> Sales { get; set; }
    public virtual DbSet<CountView> Counts { get; set; }
    public string? LastActualExecutionPlan { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=SalesDB");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AI");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.MiddleInitial).HasMaxLength(40);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.MiddleInitial).HasMaxLength(40);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SalesId)
                .HasName("SalesPK");

            entity.Property(e => e.SalesId).HasColumnName("SalesID");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.Property(e => e.SalesPersonId).HasColumnName("SalesPersonID");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesCustomersFK");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.Sales)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesProductsFK");

            entity.HasOne(d => d.SalesPerson)
                .WithMany(p => p.Sales)
                .HasForeignKey(d => d.SalesPersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesEmployeesFK");
        });

        modelBuilder
            .Entity<CountView>()
            .ToView(nameof(CountView))
            .HasNoKey();
    }
}
