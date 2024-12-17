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
                _logger.LogError("{CorrelationGuid} Unable to locate 'TouchpointId' in request header.",correlationId);
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogWarning("{CorrelationGuid} Unable to locate 'SubcontractorId' in request header", correlationId);

            Models.AdviserDetail AdviserDetailRequest;

            try
            {
                _logger.LogInformation("{CorrelationGuid} Attempt to get resource from body of the request", correlationId);
                AdviserDetailRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(ex,"{CorrelationGuid} Unable to retrieve body from req {Exception}", correlationId, ex.Message);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (AdviserDetailRequest == null)
            {
                _logger.LogError("{CorrelationGuid} Adviser Detail request is null", correlationId);
                return new UnprocessableEntityResult();
            }

            _logger.LogInformation("{CorrelationGuid} Attempt to set id's for Adviser Detail", correlationId);
            AdviserDetailRequest.SetIds(touchpointId, subcontractorId);


            _logger.LogInformation("{CorrelationGuid} Attempt to validate resource", correlationId);
            var errors = _validate.ValidateResource(AdviserDetailRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogError("{CorrelationGuid} validation errors with resource", correlationId);
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation("{CorrelationGuid} Attempting to Create Adviser Detail", correlationId);
            var adviserdetail = await _AdviserDetailPostService.CreateAsync(AdviserDetailRequest);

            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);

            return adviserdetail == null
                ? new BadRequestResult()
                : new JsonResult(adviserdetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
        }
    }
}