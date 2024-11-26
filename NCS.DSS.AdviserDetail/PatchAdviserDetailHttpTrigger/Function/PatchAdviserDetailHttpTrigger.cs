using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function
{
    public class PatchAdviserDetailHttpTrigger
    {
        private readonly IPatchAdviserDetailHttpTriggerService _adviserDetailPatchService;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger<PatchAdviserDetailHttpTrigger> _logger;
        private readonly IConvertToDynamic _convertToDynamic;
        public PatchAdviserDetailHttpTrigger(
            IPatchAdviserDetailHttpTriggerService adviserDetailPatchService,
            IValidate validate,
            IHttpRequestHelper httpRequestHelper,
            ILogger<PatchAdviserDetailHttpTrigger> logger,
            IConvertToDynamic convertToDynamic)
        {
            _adviserDetailPatchService = adviserDetailPatchService;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
            _convertToDynamic = convertToDynamic;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.AdviserDetail), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Adviser Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an Adviser Detail record.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId}")] HttpRequest req, string adviserDetailId)
        {
            var functionName = nameof(PatchAdviserDetailHttpTrigger);

            _logger.LogInformation($"Entered {functionName}");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation($"{correlationGuid} Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogWarning($"{correlationGuid} Unable to locate 'SubcontractorId' in request header");


            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                _logger.LogWarning($"{correlationGuid} Unable to parse 'adviserDetailId' to a Guid: {adviserDetailId}");
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(adviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            AdviserDetailPatch adviserDetailPatchRequest;

            try
            {
                _logger.LogInformation($"{correlationGuid} Attempt to get resource from body of the request");
                adviserDetailPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError($"{correlationGuid} Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (adviserDetailPatchRequest == null)
            {
                _logger.LogWarning($"{correlationGuid} Adviser Detail patch request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation($"{correlationGuid} Attempt to set id's for Adviser Detail patch");
            adviserDetailPatchRequest.SetIds(touchpointId, subcontractorId);

            _logger.LogInformation($"{correlationGuid} Attempt to validate resource");
            var errors = _validate.ValidateResource(adviserDetailPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _logger.LogError($"{correlationGuid} validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation($"{correlationGuid} Attempting to get Adviser Detail {adviserDetailGuid}");
            var outcome = await _adviserDetailPatchService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            if (outcome == null)
            {
                _logger.LogError($"{correlationGuid} Adviser Detail does not exist {adviserDetailGuid}");
                return new NoContentResult();
            }

            var adviserDetailResource = _adviserDetailPatchService.PatchResource(outcome, adviserDetailPatchRequest);

            if (adviserDetailResource == null)
            {
                _logger.LogError($"{correlationGuid} Adviser Detail does not exist {adviserDetailGuid}");
                return new NoContentResult();
            }

            _logger.LogInformation($"{correlationGuid} Attempting to update Adviser Detail {adviserDetailGuid}");
            var updatedAdviserDetail = await _adviserDetailPatchService.UpdateCosmosAsync(adviserDetailResource, adviserDetailGuid);

            _logger.LogInformation($"Exiting {functionName}");

            return updatedAdviserDetail == null
                ? new BadRequestObjectResult(adviserDetailGuid)
                : new JsonResult(updatedAdviserDetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}
