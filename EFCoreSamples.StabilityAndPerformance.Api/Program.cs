using EFCoreSamples.StabilityAndPerformance.Api.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// By default we are adding SQL Server DB context.
builder.Services.AddDbContextPool<SalesDbContext>(options =>
{
    // You can also use SQL Server.
    options.UseSqlServer(builder.Configuration.GetConnectionString("SalesDB"));

#if DEBUG
    // Most project shouldn't expose sensitive data, which is why we are
    // limiting to be available only in DEBUG mode.
    // If this is not, SQL "parameters" will be '?' instead of actual values.
    options.EnableSensitiveDataLogging();
#endif
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
