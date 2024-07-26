using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Function
{
    public class PostAdviserDetailHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IPostAdviserDetailHttpTriggerService _AdviserDetailPostService;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly ILogger _logger;

        public PostAdviserDetailHttpTrigger(IResourceHelper resourceHelper,
            IPostAdviserDetailHttpTriggerService AdviserDetailPostService,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            ILogger<PostAdviserDetailHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _AdviserDetailPostService = AdviserDetailPostService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _logger = logger;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.AdviserDetail),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Adviser Detail Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Adviser Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Adviser Detail for a customer.")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdviserDetails")]HttpRequest req)
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
                return new BadRequestObjectResult("Unable to locate 'APIM-TouchpointId' in request header.");
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Unable to locate 'SubcontractorId' in request header");

            Models.AdviserDetail AdviserDetailRequest;

            try
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to get resource from body of the request");
                AdviserDetailRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(_logger, correlationGuid, "Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(ex);
            }

            if (AdviserDetailRequest == null)
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Adviser Detail request is null");
                return new UnprocessableEntityResult();
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to set id's for Adviser Detail");
            AdviserDetailRequest.SetIds(touchpointId, subcontractorId);


            _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(AdviserDetailRequest, true);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Attempting to Create Adviser Detail"));
            var adviserdetail = await _AdviserDetailPostService.CreateAsync(AdviserDetailRequest);

            _loggerHelper.LogMethodExit(_logger);

            var contentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection
            {
                new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json")
            };

            return adviserdetail == null                
                ? new BadRequestResult()
                : new ObjectResult(_jsonHelper.SerializeObjectAndRenameIdProperty(adviserdetail, "id", "AdviserDetailId")) 
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    ContentTypes = contentTypes
                };
        }
    }
}