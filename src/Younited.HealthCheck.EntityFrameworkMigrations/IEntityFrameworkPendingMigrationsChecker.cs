using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    /// <summary>
    /// Identifies missing Entity Framework migrations, for the given DbContext
    /// </summary>
    /// <typeparam name="TContext">The DbContext of the database to check</typeparam>
    public interface IEntityFrameworkPendingMigrationsChecker<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Run the missing migration lookup
        /// </summary>
        /// <param name="cancellationToken">Token to cancel lookup</param>
        /// <returns>The list of missing migrations</returns>
        Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken);
    }
}