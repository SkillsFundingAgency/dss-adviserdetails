using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
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
using System.Text.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function
{
    public class PatchAdviserDetailHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchAdviserDetailHttpTriggerService _adviserDetailPatchService;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;        
        private readonly ILogger _logger;

        public PatchAdviserDetailHttpTrigger(IResourceHelper resourceHelper,
            IPatchAdviserDetailHttpTriggerService adviserDetailPatchService,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,            
            ILogger<PatchAdviserDetailHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _adviserDetailPatchService = adviserDetailPatchService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;            
            _logger = logger;
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
            _loggerHelper.LogMethodEnter(_logger);

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
                _logger.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Unable to locate 'SubcontractorId' in request header");


            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Unable to parse 'adviserDetailId' to a Guid: {0}", adviserDetailId));
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(adviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }

            Models.AdviserDetailPatch adviserDetailPatchRequest;

            try
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to get resource from body of the request");
                adviserDetailPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetailPatch>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _loggerHelper.LogError(_logger, correlationGuid, "Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(ex);
            }

            if (adviserDetailPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Adviser Detail patch request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to set id's for Adviser Detail patch");
            adviserDetailPatchRequest.SetIds(touchpointId, subcontractorId);

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(adviserDetailPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Attempting to get Adviser Detail {0}", adviserDetailGuid));
            var outcome = await _adviserDetailPatchService.GetAdviserDetailByIdAsync(adviserDetailGuid);

            if (outcome == null)
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Adviser Detail does not exist {0}", adviserDetailGuid));
                return new NoContentResult();
            }

            var adviserDetailResource = _adviserDetailPatchService.PatchResource(outcome, adviserDetailPatchRequest);

            if (adviserDetailResource == null)
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Adviser Detail does not exist {0}", adviserDetailGuid));
                return new NoContentResult();
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Attempting to update Adviser Detail {0}", adviserDetailGuid));
            var updatedAdviserDetail = await _adviserDetailPatchService.UpdateCosmosAsync(adviserDetailResource, adviserDetailGuid);

            _loggerHelper.LogMethodExit(_logger);

            return updatedAdviserDetail == null
                ? new BadRequestObjectResult(adviserDetailGuid)
                : new JsonResult(updatedAdviserDetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}
