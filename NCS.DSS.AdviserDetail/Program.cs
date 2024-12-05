using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
namespace NCS.DSS.AdviserDetail
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                    .ConfigureFunctionsWebApplication()
                     .ConfigureAppConfiguration(configBuilder =>
                     {
                         configBuilder.SetBasePath(Environment.CurrentDirectory)
                             .AddJsonFile("local.settings.json", optional: true,
                                 reloadOnChange: false)
                             .AddEnvironmentVariables();
                     })
                    .ConfigureServices((context,services) =>
                    {
                        var configuration = context.Configuration;
                        services.AddOptions<AdviserDetailConfigurationSettings>()
                            .Bind(configuration);
                        services.AddLogging();
                        services.AddApplicationInsightsTelemetryWorkerService();
                        services.ConfigureFunctionsApplicationInsights();
                        services.AddSingleton<IValidate, Validate>();
                        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                        services.AddSingleton<IJsonHelper, JsonHelper>();
                        services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                        services.AddSingleton(s =>
                        {
                            var settings = s.GetRequiredService<IOptions<AdviserDetailConfigurationSettings>>().Value;
                            var options = new CosmosClientOptions()
                            {
                                ConnectionMode = ConnectionMode.Gateway
                            };
                            return new CosmosClient(settings.AdviserDetailConnectionString, options);
                        });
                        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                        services.AddScoped<IGetAdviserDetailByIdHttpTriggerService, GetAdviserDetailByIdHttpTriggerService>();
                        services.AddScoped<IPostAdviserDetailHttpTriggerService, PostAdviserDetailHttpTriggerService>();
                        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
                        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
                        services.AddScoped<IAdviserDetailPatchService, AdviserDetailPatchService>();
                        services.AddSingleton<IConvertToDynamic, ConvertToDynamic>();
                        services.Configure<LoggerFilterOptions>(options =>
                        {
                            LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                            if (toRemove is not null)
                            {
                                options.Rules.Remove(toRemove);
                            }
                        });
                    })
                    .Build();

            await host.RunAsync();
        }
    }
}


