using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddLogging();
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddSingleton<IJsonHelper, JsonHelper>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddScoped<IGetAdviserDetailByIdHttpTriggerService, GetAdviserDetailByIdHttpTriggerService>();
        services.AddScoped<IPostAdviserDetailHttpTriggerService, PostAdviserDetailHttpTriggerService>();
        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
        services.AddScoped<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
        services.AddScoped<IAdviserDetailPatchService, AdviserDetailPatchService>();
    })
    .Build();

host.Run();
