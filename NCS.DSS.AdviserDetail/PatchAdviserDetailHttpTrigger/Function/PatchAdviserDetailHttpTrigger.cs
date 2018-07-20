using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Annotations;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.Ioc;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function
{
    public static class PatchAdviserDetailHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Adviser Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an adviser details record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, ILogger log, string adviserDetailId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchAdviserDetailHttpTriggerService adviserDetailsPatchService)
        {
            log.LogInformation("Patch Adviser Detail C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
                return HttpResponseMessageHelper.BadRequest(adviserDetailGuid);

            Models.AdviserDetailPatch adviserDetailPatchRequest;

            try
            {
                adviserDetailPatchRequest = await httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (JsonSerializationException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (adviserDetailPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            var errors = validate.ValidateResource(adviserDetailPatchRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var adviserDetail = await adviserDetailsPatchService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            if (adviserDetail == null)
                return HttpResponseMessageHelper.NoContent(adviserDetailGuid);
            
            var updatedAdviserDetail = await adviserDetailsPatchService.UpdateAsync(adviserDetail, adviserDetailPatchRequest);

            return updatedAdviserDetail == null
                ? HttpResponseMessageHelper.BadRequest(adviserDetailGuid)
                : HttpResponseMessageHelper.Ok(updatedAdviserDetail);
        }
    }
}