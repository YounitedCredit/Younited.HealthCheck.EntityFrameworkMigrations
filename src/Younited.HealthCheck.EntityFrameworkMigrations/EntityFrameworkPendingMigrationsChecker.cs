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
    public class EntityFrameworkPendingMigrationsChecker<TContext> : IEntityFrameworkPendingMigrationsChecker<TContext> where TContext : DbContext
    {
        private readonly TContext _context;

        public EntityFrameworkPendingMigrationsChecker(TContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Run the missing migration lookup
        /// </summary>
        /// <param name="cancellationToken">Token to cancel lookup</param>
        /// <returns>The list of missing migrations</returns>
        public async Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken)
        {
            return await _context.Database.GetPendingMigrationsAsync(cancellationToken);
        }
    }
}
