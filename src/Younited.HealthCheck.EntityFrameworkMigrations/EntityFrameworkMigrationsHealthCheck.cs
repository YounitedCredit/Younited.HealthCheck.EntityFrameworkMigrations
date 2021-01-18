using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    /// <summary>
    /// Validates that all Entity Framework migrations declared by the given context were executed on the server the app is connected to
    /// If any migration is missing, it will report it in the data dictionary
    /// </summary>
    /// <typeparam name="TContext">The DbContext of the database to check</typeparam>
    public class EntityFrameworkMigrationsHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
    {
        private const string SuccessStatusDescription = "All migrations were executed";
        private const string ErrorStatusDescription = "{0} migrations were not executed on the DB";

        private readonly IEntityFrameworkPendingMigrationsChecker<TContext> _checker;
        private readonly IEntityFrameworkPendingMigrationsCheckerStorage<TContext> _storage;

        public EntityFrameworkMigrationsHealthCheck(IEntityFrameworkPendingMigrationsChecker<TContext> checker, IEntityFrameworkPendingMigrationsCheckerStorage<TContext> storage)
        {
            _checker = checker;
            _storage = storage;
        }

        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            // as soon as we confirmed once that all migrations were successfully executed, there's no reason to rerun the check
            // we assume here that once the missing migration has been executed, it won't be rolled back
            if (_storage.LastExecutionResult)
                return HealthCheckResult.Healthy(SuccessStatusDescription);

            var result = (await _checker.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (!result.Any())
            {
                // Store in the persistent storage that execution has been a success, to avoid any further throughout the process lifetime
                _storage.LastExecutionResult = true;
                return HealthCheckResult.Healthy(SuccessStatusDescription);
            }

            return HealthCheckResult.Unhealthy(
                string.Format(ErrorStatusDescription, result.Count),
                data: new Dictionary<string, object>()
                {
                    {"MissingMigrations", result}
                });
        }
    }
}
