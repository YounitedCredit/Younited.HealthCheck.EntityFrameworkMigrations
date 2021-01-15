using Microsoft.EntityFrameworkCore;

namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    /// <summary>
    /// Persistent storage for health check
    /// </summary>
    public class EntityFrameworkPendingMigrationsCheckerStorage<TContext> : IEntityFrameworkPendingMigrationsCheckerStorage<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Status of the last health check computation
        /// </summary>
        public bool LastExecutionResult { get; set; }
    }
}