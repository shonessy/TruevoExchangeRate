using Microsoft.EntityFrameworkCore;
using TruevoExchangeRateAPI.Data.Models;

namespace TruevoExchangeRateAPI.Data
{
    public class TruevoExchangeRateDbContext : DbContext
    {
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        public TruevoExchangeRateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
