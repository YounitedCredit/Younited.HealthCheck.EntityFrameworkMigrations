using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Younited.HealthCheck.EntityFrameworkMigrations.Sample.Context;

namespace Younited.HealthCheck.EntityFrameworkMigrations.Sample
{
    public partial class Startup
    {
        private void ConfigureHealthCheck(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<SampleDbContext>()
                .AddEntityFrameworkMigrationsCheck<SampleDbContext>() // Registers required services and check the pipeline

                // And all the other checks you may need for your app
                .AddCheck<PingHealthChecker>(nameof(PingHealthChecker));

            AddHealthChecksUi(services);
        }

        private void AddHealthChecksUi(IServiceCollection services)
        {
            services.AddHealthChecksUI(setupSettings: settings =>
            {
                settings.AddHealthCheckEndpoint("Sample WebApp", $"https://localhost:5001/health");
                settings.SetEvaluationTimeInSeconds(10);
            });
        }

        private void MapAndUseHealthChecksUi(IApplicationBuilder app)
        {
            app
                .UseStaticFiles()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    endpoints.MapHealthChecksUI();
                })
                .UseHealthChecksUI();
        }
    }

    public class PingHealthChecker : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(HealthCheckResult.Healthy("Ping Check"));
        }
    }
}