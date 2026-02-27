using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TP_DisenoDB.Infrastructure.Mongo.Collections;
using System.Collections.Generic;

namespace TP_DisenoDB.IntegrationTests.Fixtures;

public class NoRelationalTestFixture : WebApplicationFactory<Program>
{
    public string DbName { get; } = $"test_db_{Guid.NewGuid().ToString().Replace("-", "")}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Esta es la forma correcta de sobreescribir appsettings.json en tests
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Persistence", "Mongo" },
                { "ConnectionStrings:Mongo", "mongodb://root:root@localhost:27017/?authSource=admin" },
                { "ConnectionStrings:MongoDbName", DbName }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext to avoid initialization issues
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<TP_DisenoDB.Infrastructure.Relational.DbContext.MySqlDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // IMPORTANT: Remove the MySQL Service if it was registered
            var serviceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TP_DisenoDB.Application.Interfaces.IPaymentService));
            if (serviceDescriptor != null) services.Remove(serviceDescriptor);

            // Remove real MongoContext if it exists
            var mongoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MongoContext));
            if (mongoDescriptor != null) services.Remove(mongoDescriptor);

            // Add Test MongoContext and Mongo implementation
            var connectionString = "mongodb://root:root@localhost:27017/?authSource=admin"; 
            services.AddSingleton<MongoContext>(sp => new MongoContext(connectionString, DbName));
            services.AddScoped<TP_DisenoDB.Application.Interfaces.IPaymentService, TP_DisenoDB.Application.Services.MongoPaymentService>();

            // Add a dummy DbContextOptions just to satisfy DI if something tries to resolve it
            services.AddDbContext<TP_DisenoDB.Infrastructure.Relational.DbContext.MySqlDbContext>(options => {
                options.UseInMemoryDatabase("DummyForMongoTest");
            });
        });
    }
}
