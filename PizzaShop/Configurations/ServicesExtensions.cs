using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace PizzaShop.Configurations
{
    public static class ServicesExtensions
    {
        public static void ConfigureMonitoring(this WebApplicationBuilder builder)
        {
            const string serviceName = "Pizza Shop";
            // L'URL du collecteur (le point d'entrée unique pour tes données)
            string otlpExporter = builder.Configuration["OtlpExporter"] ?? "http://host.docker.internal:4317";

            // --- CONFIGURATION OPENTELEMETRY ---
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))

                // SECTION PROMETHEUS (Métriques)
                // Définit CE QUE l'on mesure (CPU, RAM, requêtes HTTP)
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation() // Mesure le temps de réponse de tes APIs
                    .AddHttpClientInstrumentation() // Mesure les appels vers des APIs externes
                    .AddRuntimeInstrumentation()    // Mesure la santé du moteur .NET (GC, Threads)
                    .AddPrometheusExporter())       // Expose le tout au format Prometheus sur /metrics

                // SECTION TEMPO (Traces)
                // Définit COMMENT on suit une requête de A à Z (le "fil d'Ariane")
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options => 
                    {
                        options.RecordException = true; 
                    })
                    .AddSqlClientInstrumentation(options => 
                    {
                        options.EnrichWithSqlCommand = (activity, command) =>
                        {
                            if (command is Microsoft.Data.SqlClient.SqlCommand sqlCommand)
                            {
                                activity.SetTag("db.statement", sqlCommand.CommandText);
                            }
                        };
                    })
                    .AddHttpClientInstrumentation() // Continue la trace si ton app appelle une autre API
                    .AddSource("Microsoft.AspNetCore.Components.Server") // Pour Blazor Server interne
                    .AddSource("MudBlazor")
                    .AddOtlpExporter(opt => {
                        opt.Endpoint = new Uri(otlpExporter); // Envoie les traces à Tempo via gRPC
                    }));

            // SECTION LOKI (Logs via Serilog)
            // Serilog est utilisé ici comme "transporteur" pour les logs
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext() // Capture les IDs de corrélation (TraceId)
                .Enrich.WithProperty("service_name", serviceName)
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpExporter;
                    options.Protocol = OtlpProtocol.Grpc;
                    // Ces attributs aident Loki à indexer tes logs pour des recherches rapides
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["index_label"] = "pizza-shop-logs"
                    };
                })
                .CreateLogger();

            // Remplace le système de log par défaut de .NET par Serilog
            builder.Host.UseSerilog();
        }
    }
}
