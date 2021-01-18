using Microsoft.EntityFrameworkCore;

namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    /// <summary>
    /// Persistent storage for health check
    /// </summary>
    public interface IEntityFrameworkPendingMigrationsCheckerStorage<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Status of the last health check computation
        /// </summary>
        bool LastExecutionResult { get; set; }
    }
}