using Microsoft.EntityFrameworkCore;
using Younited.HealthCheck.EntityFrameworkMigrations.Sample.Model;

namespace Younited.HealthCheck.EntityFrameworkMigrations.Sample.Context
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
        {
        }

        public DbSet<SampleEntity> SampleEntities { get; set; }
    }
}
