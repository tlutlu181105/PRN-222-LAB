using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FCanteen.Data;

public class FCanteenContextFactory : IDesignTimeDbContextFactory<FCanteenContext>
{
    public FCanteenContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("FCanteenConnection");

        var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new FCanteenContext(optionsBuilder.Options);
    }
}