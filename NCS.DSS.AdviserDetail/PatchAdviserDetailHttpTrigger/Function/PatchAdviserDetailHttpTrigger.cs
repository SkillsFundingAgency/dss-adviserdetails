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

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

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
                _logger.LogError("{CorrelationGuid} Unable to locate 'TouchpointId' in request header.", correlationGuid);
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogWarning("{CorrelationGuid} Unable to locate 'SubcontractorId' in request header", correlationGuid);


            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                _logger.LogError("{CorrelationGuid} Unable to parse 'adviserDetailId' to a Guid: {AdviserDetailId}", correlationGuid, adviserDetailId);
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(adviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            AdviserDetailPatch adviserDetailPatchRequest;

            try
            {
                _logger.LogInformation("{CorrelationGuid} Attempt to get resource from body of the request", correlationGuid);
                adviserDetailPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(ex,"{CorrelationGuid} Unable to retrieve body from req {Exception}", correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (adviserDetailPatchRequest == null)
            {
                _logger.LogError("{CorrelationGuid} Adviser Detail patch request is null", correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("{CorrelationGuid} Attempt to set id's for Adviser Detail patch", correlationGuid);
            adviserDetailPatchRequest.SetIds(touchpointId, subcontractorId);

            _logger.LogInformation("{CorrelationGuid} Attempt to validate resource", correlationGuid);
            var errors = _validate.ValidateResource(adviserDetailPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _logger.LogError("{CorrelationGuid} validation errors with resource", correlationGuid);
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation("{CorrelationGuid} Attempting to get Adviser Detail {AdviserDetailId}", correlationGuid, adviserDetailGuid);
            var outcome = await _adviserDetailPatchService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            if (outcome == null)
            {
                _logger.LogError("{CorrelationGuid} Adviser Detail does not exist {AdviserDetailId}", correlationGuid);
                return new NoContentResult();
            }

            var adviserDetailResource = _adviserDetailPatchService.PatchResource(outcome, adviserDetailPatchRequest);

            if (adviserDetailResource == null)
            {
                _logger.LogError("{CorrelationGuid} Adviser Detail does not exist {AdviserDetailId}", correlationGuid, adviserDetailGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("{CorrelationGuid} Attempting to update Adviser Detail {AdviserDetailId}", correlationGuid, adviserDetailGuid);
            var updatedAdviserDetail = await _adviserDetailPatchService.UpdateCosmosAsync(adviserDetailResource, adviserDetailGuid);

            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);

            return updatedAdviserDetail == null
                ? new BadRequestObjectResult(adviserDetailGuid)
                : new JsonResult(updatedAdviserDetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}
