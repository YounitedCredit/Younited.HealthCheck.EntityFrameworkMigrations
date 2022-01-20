using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Younited.HealthCheck.EntityFrameworkMigrations
{
    public static class EntityFrameworkMigrationsHealthCheckExtensions
    {
        /// <summary>
        /// Registers a check that validates that all Entity Framework migrations declared by the given context were executed on the server the app is connected to
        /// If any migration is missing, it will report it in the data dictionary
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="builder"></param>
        /// <param name="checkName"></param>
        /// <param name="failureStatus"></param>
        /// <param name="timeout"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddEntityFrameworkMigrationsCheck<TContext>(this IHealthChecksBuilder builder, string checkName = null, HealthStatus? failureStatus = null, TimeSpan? timeout =  null, params string[] tags)
            where TContext : DbContext
        {
            // PendingMigrationsCheckerStorage is registered as a singleton, to provide persistent memory and remember whether last check was successful or not
            builder.Services.TryAddSingleton<IEntityFrameworkPendingMigrationsCheckerStorage<TContext>, EntityFrameworkPendingMigrationsCheckerStorage<TContext>>();

            // the scope created here will be disposed by the parent scope
            // todo fix when issue is resolved https://github.com/dotnet/aspnetcore/issues/14453
            builder.Services.TryAddScoped<IEntityFrameworkPendingMigrationsChecker<TContext>>(provider => new EntityFrameworkPendingMigrationsChecker<TContext>(provider.CreateScope().ServiceProvider.GetRequiredService<TContext>()));

            builder.AddCheck<EntityFrameworkMigrationsHealthCheck<TContext>>(checkName ?? $"{typeof(TContext).Name}MigrationsHealthCheck", failureStatus, tags, timeout);

            return builder;
        }
    }
}