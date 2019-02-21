using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.GetAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;


namespace NCS.DSS.AdviserDetail.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetAdviserDetailHttpTriggerService, GetAdviserDetailHttpTriggerService>();
            services.AddTransient<IGetAdviserDetailByIdHttpTriggerService, GetAdviserDetailByIdHttpTriggerService>();
            services.AddTransient<IPostAdviserDetailHttpTriggerService, PostAdviserDetailHttpTriggerService>();
            services.AddTransient<IPatchAdviserDetailHttpTriggerService, PatchAdviserDetailHttpTriggerService>();
            services.AddTransient<IAdviserDetailPatchService, AdviserDetailPatchService>();

            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();

            return services.BuildServiceProvider(true);
        }
    }
}
