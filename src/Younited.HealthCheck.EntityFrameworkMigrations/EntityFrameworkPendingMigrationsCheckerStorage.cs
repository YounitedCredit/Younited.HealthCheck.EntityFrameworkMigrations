namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    /// <summary>
    /// Persistent storage for health check
    /// </summary>
    public class EntityFrameworkPendingMigrationsCheckerStorage : IEntityFrameworkPendingMigrationsCheckerStorage
    {
        /// <summary>
        /// Status of the last health check computation
        /// </summary>
        public bool LastExecutionResult { get; set; }
    }
}