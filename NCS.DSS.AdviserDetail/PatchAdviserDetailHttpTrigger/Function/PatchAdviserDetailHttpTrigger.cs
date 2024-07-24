using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using System.Text;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function
{
    public class PatchAdviserDetailHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IPatchAdviserDetailHttpTriggerService _adviserDetailPatchService;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public PatchAdviserDetailHttpTrigger(IResourceHelper resourceHelper,
            IPatchAdviserDetailHttpTriggerService adviserDetailPatchService,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _adviserDetailPatchService = adviserDetailPatchService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.AdviserDetail), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an Adviser Detail record.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId}")] HttpRequest req, ILogger log, string adviserDetailId)
        {
            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");


            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'adviserDetailId' to a Guid: {0}", adviserDetailId));
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(adviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            Models.AdviserDetailPatch adviserDetailPatchRequest;

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                adviserDetailPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(ex);
            }

            if (adviserDetailPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Adviser Detail patch request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for Adviser Detail patch");
            adviserDetailPatchRequest.SetIds(touchpointId, subcontractorId);

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(adviserDetailPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Adviser Detail {0}", adviserDetailGuid));
            var outcome = await _adviserDetailPatchService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            if (outcome == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Adviser Detail does not exist {0}", adviserDetailGuid));
                return new NoContentResult();
            }

            var adviserDetailResource = _adviserDetailPatchService.PatchResource(outcome, adviserDetailPatchRequest);

            if (adviserDetailResource == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Adviser Detail does not exist {0}", adviserDetailGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update Adviser Detail {0}", adviserDetailGuid));
            var updatedAdviserDetail = await _adviserDetailPatchService.UpdateCosmosAsync(adviserDetailResource, adviserDetailGuid);

            _loggerHelper.LogMethodExit(log);

            return updatedAdviserDetail == null ?
                new BadRequestObjectResult(adviserDetailGuid) :
                new OkObjectResult(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedAdviserDetail, "id", "AdviserDetailId"));
        }
    }
}
