using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Annotations;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.Ioc;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Function
{
    public static class GetAdviserDetailByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the adviser details for a given interaction.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, ILogger log, string adviserDetailId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IGetAdviserDetailByIdHttpTriggerService getAdviserDetailByIdService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (touchpointId == null)
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Adviser Detail By Id C# HTTP trigger function  processed a request. " + touchpointId);

            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
                return HttpResponseMessageHelper.BadRequest(adviserDetailGuid);

            var adviserDetail = await getAdviserDetailByIdService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            return adviserDetail == null
                ? HttpResponseMessageHelper.NoContent(adviserDetailGuid)
                : HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(adviserDetail));
        }
    }
}