using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function
{
    public static class PatchAdviserDetailHttpTrigger
    {
        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.AdviserDetail),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an Adviser Detail record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId}")]HttpRequest req, ILogger log, string adviserDetailId,
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IPatchAdviserDetailHttpTriggerService AdviserDetailPatchService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");


            if (!Guid.TryParse(adviserDetailId, out var AdviserDetailGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'adviserDetailId' to a Guid: {0}", adviserDetailId));
                return httpResponseMessageHelper.BadRequest(AdviserDetailGuid);
            }

            Models.AdviserDetailPatch AdviserDetailPatchRequest;

            try
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                AdviserDetailPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (AdviserDetailPatchRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Adviser Detail patch request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for Adviser Detail patch");
            AdviserDetailPatchRequest.SetIds(touchpointId, subcontractorId);

            
            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Adviser Detail", AdviserDetailGuid));
            var outcome = await AdviserDetailPatchService.GetAdviserDetailByIdAsync(AdviserDetailGuid);

            if (outcome == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Adviser Detail does not exist {0}", AdviserDetailGuid));
                return httpResponseMessageHelper.NoContent(AdviserDetailGuid);
            }
            
            var adviserdetailResource = AdviserDetailPatchService.PatchResource(outcome, AdviserDetailPatchRequest);

            if (adviserdetailResource == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Adviser Detail does not exist {0}", AdviserDetailGuid));
                return httpResponseMessageHelper.NoContent(AdviserDetailGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(adviserdetailResource, false);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Adviser Detail {0}", AdviserDetailGuid));
            var updatedAdviserDetail = await AdviserDetailPatchService.UpdateCosmosAsync(adviserdetailResource);


            loggerHelper.LogMethodExit(log);

            return updatedAdviserDetail == null ?
                httpResponseMessageHelper.BadRequest(AdviserDetailGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(updatedAdviserDetail, "id", "AdviserDetailId"));

        }
    }
}
