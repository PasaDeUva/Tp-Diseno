using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization;
using TP_DisenoDB.Infrastructure.Relational.DbContext;
using TP_DisenoDB.Infrastructure.Mongo.Collections;
using TP_DisenoDB.Domain.Entities;
using TP_DisenoDB.Application.Interfaces;
using TP_DisenoDB.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var persistence = builder.Configuration["Persistence"] ?? "MySql";

if (persistence == "MySql")
{
    var connectionString = builder.Configuration.GetConnectionString("MySql");
    builder.Services.AddDbContext<MySqlDbContext>(options =>
    {
        options.UseLazyLoadingProxies();
        if (builder.Environment.IsEnvironment("Testing"))
        {
            options.UseMySql("Server=localhost;Database=dummy;", new MySqlServerVersion(new Version(8, 0)));
        }
        else
        {
            options.UseMySql(connectionString!, ServerVersion.AutoDetect(connectionString));
        }
    });
    builder.Services.AddScoped<IPaymentService, MySqlPaymentService>();
    builder.Services.AddScoped<IDbSeeder, TP_DisenoDB.Infrastructure.Relational.Data.MySqlDbSeeder>();
}
else if (persistence == "Mongo")
{
    var connectionString = builder.Configuration.GetConnectionString("Mongo");
    var dbName = builder.Configuration["ConnectionStrings:MongoDbName"] ?? "tp_disenodb";

    if (!BsonClassMap.IsClassMapRegistered(typeof(Promotion)))
    {
        BsonClassMap.RegisterClassMap<Promotion>(cm => {
            cm.AutoMap();
            cm.SetIsRootClass(true);
        });
        BsonClassMap.RegisterClassMap<DiscountPromotion>();
        BsonClassMap.RegisterClassMap<FinancingPromotion>();
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(Purchase)))
    {
        BsonClassMap.RegisterClassMap<Purchase>(cm => {
            cm.AutoMap();
            cm.SetIsRootClass(true);
        });
        BsonClassMap.RegisterClassMap<CashPurchase>();
        BsonClassMap.RegisterClassMap<MonthlyPayments>();
    }

    builder.Services.AddSingleton<MongoContext>(sp => new MongoContext(connectionString!, dbName));
    builder.Services.AddScoped<IPaymentService, MongoPaymentService>();
    builder.Services.AddScoped<IDbSeeder, TP_DisenoDB.Infrastructure.Mongo.Data.MongoDbSeeder>();
}

var app = builder.Build();

// Execute Seeding
using (var scope = app.Services.CreateScope())
{
    var persistenceMode = app.Configuration["Persistence"] ?? "MySql";
    Console.WriteLine($"[Seeding] Starting process. Mode: {persistenceMode}");

    if (persistenceMode == "MySql")
    {
        var context = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    var seeder = scope.ServiceProvider.GetService<IDbSeeder>();
    if (seeder != null)
    {
        try 
        {
            Console.WriteLine($"[Seeding] Executing {seeder.GetType().Name}...");
            await seeder.SeedAsync();
            Console.WriteLine("[Seeding] Completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeding] ERROR: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Seeding] Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
    else
    {
        Console.WriteLine("[Seeding] No seeder registered for the current persistence mode.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
