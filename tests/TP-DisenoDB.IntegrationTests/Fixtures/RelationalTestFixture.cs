using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TP_DisenoDB.Infrastructure.Relational.DbContext;
using System.Collections.Generic;

namespace TP_DisenoDB.IntegrationTests.Fixtures;

public class RelationalTestFixture : WebApplicationFactory<Program>
{
    public string DbName { get; } = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Persistence", "MySql" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MySqlDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<MySqlDbContext>(options =>
            {
                options.UseInMemoryDatabase(DbName);
                options.UseLazyLoadingProxies();
            });
        });
    }
}
