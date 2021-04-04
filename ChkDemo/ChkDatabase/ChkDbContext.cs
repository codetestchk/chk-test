using Microsoft.EntityFrameworkCore;
using ChkDatabase.Entites;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ChkDatabase
{
    public class ChkDbContext : DbContext
    {
        public ChkDbContext()
        {
            // Empty constructor needed for design time creation, see : https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#using-a-constructor-with-no-parameters

        }

        public ChkDbContext(DbContextOptions<ChkDbContext> options) : base(options)
        {
        }

        public DbSet<ChkMerchant> Merchants { get; set; }
        public DbSet<ChkTransaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("ChkConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
