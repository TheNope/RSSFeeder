using System;
using System.Threading;
using Application.Common.Exceptions.Neo4j;
using Application.Common.Interfaces;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repositories.EFCore;
using Infrastructure.Persistence.Repositories.Neo4j;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;
using OPD.Health;

namespace Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        private static int connectionAttempts = 0;
        private static IConfiguration _configuration;
        
        public static IServiceCollection AddDataBaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var _configuration = configuration;
            
            // ----------------- MySQL ------------------

            const string migrationsAssembly = "Api";

            var con = configuration.GetSection("Database");

            var connectionString = $"Server={con["Server"]};Database={con["Scheme"]};Uid={con["Username"]};Pwd={con["Password"]};Port={con["Port"]};";

            services.AddDbContext<DatabaseContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
                sql => sql
                    .MigrationsAssembly(migrationsAssembly)
                    .EnableRetryOnFailure()
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                ));
            services.AddHealthChecks().AddMySql(connectionString, name: "MySQL", tags: new[] { "startup" });

            services.AddScoped<IDatabaseContext>(provider => provider.GetService<DatabaseContext>());
            services.AddScoped<ISampleRepository, EFCoreSampleRepository>();

            // ----------------- neo4j ------------------

            var configNeo4j = configuration.GetSection("Neo4j");

            var neo4j = new BoltGraphClient(new Uri(configNeo4j["Server"]), configNeo4j["Username"], configNeo4j["Password"])
            {
                DefaultDatabase = configNeo4j["Scheme"]
            };

            CreateNeo4jConnection(neo4j);
            
            services.AddSingleton<IBoltGraphClient>(neo4j);

            services.AddScoped<IAssetRepository, Neo4jAssetRepository>();
            
            services.AddHealthChecks()
                .AddCheck<Neo4jConnectionHealthCheck>(name: "Neo4j - Connection",tags: new[] { "startup" })
                .AddCheck<Neo4jApocHealthCheck>(name: "Neo4j - APOC", tags: new[] { "startup" });
            
            return services;
        }
        
        public static void CreateNeo4jConnection( BoltGraphClient client)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] [Neo4J] Connecting...");
            try
            {
                connectionAttempts++;

                if (connectionAttempts >= 5)
                    throw new Neo4jConnectionFailedException($"[Neo4J] Connection attempt failed too often: {connectionAttempts}, " +
                                                             $"{_configuration["Neo4j:Scheme"]},{_configuration["Neo4j:Server"]},{_configuration["Neo4j:Username"]}");

                client.ConnectAsync().Wait();
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] [Neo4J] Connection established!");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [Neo4J] Establishing a connection failed! {ex}");
                Thread.Sleep(5000);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARN] [Neo4J] Retrying to connect...");

                CreateNeo4jConnection(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [Neo4J] {ex.Message}");
                throw new Neo4jConnectionFailedException($"{ex.Message}");
            }
        }
    }
}