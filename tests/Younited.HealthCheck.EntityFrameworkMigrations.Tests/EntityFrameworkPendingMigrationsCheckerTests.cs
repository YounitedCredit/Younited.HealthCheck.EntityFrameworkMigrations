using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Younited.HealthCheck.EntityFrameworkMigrations.Sample.Context;

namespace Younited.HealthCheck.EntityFrameworkMigrations.Tests
{
    public class EntityFrameworkPendingMigrationsCheckerTests
    {
        private DbConnection _connection;
        private SampleDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // Create an empty in memory sqlite DB
            _context = new SampleDbContext(
                new DbContextOptionsBuilder<SampleDbContext>()
                .UseSqlite(_connection)
                .Options);
        }

        [TearDown]
        public void TearDown()
        {
            // Removes the in memory sqlite DB
            _connection.Dispose();
        }

        [Test]
        public void dbcontext_should_have_one_migration()
        {
            // To be consistent, these unit test require the used dbcontext to have one existing migration
            Assert.That(_context.Database.GetMigrations(), Is.EquivalentTo(new[] { "20200531210738_MissingMigration" }));
        }
        
        [Test]
        public async Task No_missing_migration_should_be_identified_when_all_migrations_were_executed()
        {
            var checker = new EntityFrameworkPendingMigrationsChecker<SampleDbContext>(_context);

            // Run all the migrations, so that DB is up to date
            _context.Database.Migrate();

            var result = (await checker.GetPendingMigrationsAsync(CancellationToken.None)).ToList();
            
            // No migrations should be identified as missing
            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task One_missing_migration_should_be_identified_when_the_only_db_migration_has_not_been_executed()
        {
            var checker = new EntityFrameworkPendingMigrationsChecker<SampleDbContext>(_context);

            // Db is brand new, we ran no migrations

            var result = (await checker.GetPendingMigrationsAsync(CancellationToken.None)).ToList();

            // No migrations should be identified as missing
            Assert.That(result, Is.EquivalentTo(new []{"20200531210738_MissingMigration"}));
        }
    }
}