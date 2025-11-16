using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Ecommerce.Infrastructure.DesignTime
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Point to the Api project folder so appsettings.json is found
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Ecommerce.Api");

            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(basePath, "appsettings.Development.json"), optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            _ = optionsBuilder.UseSqlServer(cs);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
