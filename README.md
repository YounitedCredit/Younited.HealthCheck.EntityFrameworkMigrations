# Younited Credit Entity Framework healthcheck library

The purpose of this project is to add a validation that all Entity Framework migrations have been executed, for a given DbContext, to the list of webapi healtchchecks collection

# Getting Started
1. Setup health checks for your webapp, as explained in this [article](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1)

2. Add reference to the **Younited.HealthCheck.EntityFrameworkMigrations** nuget to your webapp project

3. Publish a health check for your EntityFramework DbContext
```cs
services.AddHealthChecks()
    .AddEntityFrameworkMigrationsCheck<SampleDbContext>() // Registers required services and check the pipeline

    // And all the other checks you may need for your app
    .AddCheck<PingHealthChecker>(nameof(PingHealthChecker));
```

You can refer to the sample app setup for further reference

Once the setup is done, you shall receive missing migrations as part of the health detailed status

```json
{
    "status": "Unhealthy",
    "totalDuration": "00:00:05.9933318",
    "entries": {
        
        // List of missing migrations, if any
        "SampleDbContextMigrationsHealthCheck": {
            "data": {
                "MissingMigrations": [
                    "20200531210738_MissingMigration"
                ]
            },
            "description": "1 migrations were not executed on the DB",
            "duration": "00:00:05.9883966",
            "status": "Unhealthy"
        },

        // Other health checks results
        "PingHealthChecker": {
            "data": {},
            "description": "Ping Check",
            "duration": "00:00:00.0001120",
            "status": "Healthy"
        }
    }
}
```