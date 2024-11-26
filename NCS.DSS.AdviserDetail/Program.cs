using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddApplicationInsightsTelemetryWorkerService();
                        services.ConfigureFunctionsApplicationInsights();
                        services.AddSingleton<IValidate, Validate>();
                        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                        services.AddSingleton<IJsonHelper, JsonHelper>();
                        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
                        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                        services.AddScoped<IGetAdviserDetailByIdHttpTriggerService, GetAdviserDetailByIdHttpTriggerService>();
                        services.AddScoped<IPostAdviserDetailHttpTriggerService, PostAdviserDetailHttpTriggerService>();
                        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
                        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
                        services.AddScoped<IAdviserDetailPatchService, AdviserDetailPatchService>();
                        services.AddSingleton<IConvertToDynamic, ConvertToDynamic>();
                        services.Configure<LoggerFilterOptions>(options =>
                        {
                            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
                            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
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


