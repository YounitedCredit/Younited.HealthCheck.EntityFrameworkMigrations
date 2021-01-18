using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Younited.HealthCheck.EntityFrameworkMigrations.Tests
{
    public class EntityFrameworkMigrationsHealthCheckTests
    {
        [Test]
        public async Task Healthy_status_should_be_returned_when_check_finds_no_missing_migration()
        {
            var cancellationToken = CancellationToken.None;

            var migrationsChecker = Substitute.For<IEntityFrameworkPendingMigrationsChecker<DbContext>>();
            var check = new EntityFrameworkMigrationsHealthCheck<DbContext>(migrationsChecker, new EntityFrameworkPendingMigrationsCheckerStorage<DbContext>());

            // The checker confirms that no migration is missing : it returns an empty list
            migrationsChecker.GetPendingMigrationsAsync(cancellationToken).Returns(new string[0]);

            var result = await check.CheckHealthAsync(new HealthCheckContext(), cancellationToken);

            // Returned status is healthy
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Is.EqualTo("All migrations were executed"));
        }

        [Test]
        public async Task Unhealthy_status_should_be_returned_when_check_finds_at_least_one_missing_migration()
        {
            var cancellationToken = CancellationToken.None;

            var migrationsChecker = Substitute.For<IEntityFrameworkPendingMigrationsChecker<DbContext>>();
            var check = new EntityFrameworkMigrationsHealthCheck<DbContext>(migrationsChecker, new EntityFrameworkPendingMigrationsCheckerStorage<DbContext>());

            // The checker identifies two missing migrations
            var missingMigrations = new[] { "MissingMigration1", "MissingMigration2" };
            migrationsChecker.GetPendingMigrationsAsync(cancellationToken).Returns(missingMigrations);

            var result = await check.CheckHealthAsync(new HealthCheckContext(), cancellationToken);

            // Returned status is unhealthy
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
            Assert.That(result.Description, Is.EqualTo("2 migrations were not executed on the DB"));

            // And the data dictionary holds the list of missing migrations
            Assert.That(result.Data, Contains.Key("MissingMigrations"));
            Assert.That(result.Data["MissingMigrations"], Is.EqualTo(missingMigrations));
        }

        [Test]
        public async Task Missing_migrations_should_be_checked_each_time_until_no_missing_migration_is_found()
        {
            var storageSingleton = new EntityFrameworkPendingMigrationsCheckerStorage<DbContext>();
            var cancellationToken = CancellationToken.None;
            var expectedMigrations = new List<string>() { "migration1", "migration2" };

            var migrationsChecker = Substitute.For<IEntityFrameworkPendingMigrationsChecker<DbContext>>();
            var check = new EntityFrameworkMigrationsHealthCheck<DbContext>(migrationsChecker, storageSingleton);

            // the checker will return the same 2 missing migrations
            migrationsChecker.GetPendingMigrationsAsync(cancellationToken).Returns(expectedMigrations);

            for (var i = 0; i < 100; i++)
            {
                var unhealthyResult = await check.CheckHealthAsync(new HealthCheckContext(), cancellationToken);

                // The unhealthy status should report the details of the missing migrations
                Assert.That(unhealthyResult.Status, Is.EqualTo(HealthStatus.Unhealthy));

                // the last execution result should remain false, to force the next health check to perform the missing migration lookup again
                Assert.That(storageSingleton.LastExecutionResult, Is.False);
            }

            // as the migrations checker identifies missing migrations, it should be called again for each health check
            // the missing migrations must not be put in cache by the check
            await migrationsChecker.Received(100).GetPendingMigrationsAsync(cancellationToken);

            // Now the migration checker behaves as if all the migrations were executed
            migrationsChecker.ClearSubstitute();
            migrationsChecker.GetPendingMigrationsAsync(cancellationToken).Returns(new string[0]);

            // the status reported by the check is now healthy
            var healthyResult = await check.CheckHealthAsync(new HealthCheckContext(), cancellationToken);
            Assert.That(healthyResult.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(healthyResult.Description, Is.EqualTo("All migrations were executed"));

            // the migrations checker was executed, once again
            await migrationsChecker.Received(1).GetPendingMigrationsAsync(cancellationToken);

            // now that the check completed successfully once, the last execution result should be true, to avoid any further call to the migrations checker
            Assert.That(storageSingleton.LastExecutionResult, Is.True);

            migrationsChecker.ClearReceivedCalls();
            for (var i = 0; i < 100; i++)
            {
                // All further call should return a healthy status
                var newHealthyResult = await check.CheckHealthAsync(new HealthCheckContext(), cancellationToken);
                Assert.That(newHealthyResult.Status, Is.EqualTo(HealthStatus.Healthy));
            }

            // and the migrations checker should not be called any more, now that one run did succeed
            await migrationsChecker.DidNotReceiveWithAnyArgs().GetPendingMigrationsAsync(default);
        }
    }
}