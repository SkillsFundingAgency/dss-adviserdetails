using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Function
{
    public class PostAdviserDetailHttpTrigger
    {
        private readonly IPostAdviserDetailHttpTriggerService _AdviserDetailPostService;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger<PostAdviserDetailHttpTrigger> _logger;
        private readonly IConvertToDynamic _convertToDynamic;
        public PostAdviserDetailHttpTrigger(
            IPostAdviserDetailHttpTriggerService AdviserDetailPostService,
            IValidate validate,
            IHttpRequestHelper httpRequestHelper,
            ILogger<PostAdviserDetailHttpTrigger> logger,
            IConvertToDynamic convertToDynamic)
        {
            _AdviserDetailPostService = AdviserDetailPostService;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
            _convertToDynamic = convertToDynamic;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.AdviserDetail), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Adviser Detail Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Adviser Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Adviser Detail for a customer.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdviserDetails")] HttpRequest req)
        {
            var functionName = nameof(PostAdviserDetailHttpTrigger);

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
                _logger.LogError($"{correlationGuid} Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult("Unable to locate 'APIM-TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogWarning($"{correlationGuid} Unable to locate 'SubcontractorId' in request header");

            Models.AdviserDetail AdviserDetailRequest;

            try
            {
                _logger.LogInformation($"{correlationGuid} Attempt to get resource from body of the request");
                AdviserDetailRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError($"{correlationGuid} Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (AdviserDetailRequest == null)
            {
                _logger.LogError($"{correlationGuid} Adviser Detail request is null");
                return new UnprocessableEntityResult();
            }

            _logger.LogInformation($"{correlationGuid} Attempt to set id's for Adviser Detail");
            AdviserDetailRequest.SetIds(touchpointId, subcontractorId);


            _logger.LogInformation($"{correlationGuid} Attempt to validate resource");
            var errors = _validate.ValidateResource(AdviserDetailRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogError($"{correlationGuid} validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation($"{correlationGuid} Attempting to Create Adviser Detail");
            var adviserdetail = await _AdviserDetailPostService.CreateAsync(AdviserDetailRequest);

            _logger.LogInformation($"Exiting {functionName}");

            return adviserdetail == null
                ? new BadRequestResult()
                : new JsonResult(adviserdetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
        }
    }
}