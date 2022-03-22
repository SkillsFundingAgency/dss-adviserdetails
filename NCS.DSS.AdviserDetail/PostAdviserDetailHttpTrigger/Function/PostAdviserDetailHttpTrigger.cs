using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Function
{
    public class PostAdviserDetailHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IPostAdviserDetailHttpTriggerService _AdviserDetailPostService;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public PostAdviserDetailHttpTrigger(IResourceHelper resourceHelper,
            IPostAdviserDetailHttpTriggerService AdviserDetailPostService,
            IValidate validate,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _AdviserDetailPostService = AdviserDetailPostService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.AdviserDetail),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Adviser Detail Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Outcome validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Adviser Detail for a customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdviserDetails")]HttpRequest req, ILogger log)
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
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                log.LogInformation("Unable to locate 'APIM-SubcontractorId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            Models.AdviserDetail AdviserDetailRequest;

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                AdviserDetailRequest = await _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (AdviserDetailRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Adviser Detail request is null");
                return _httpResponseMessageHelper.UnprocessableEntity();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for Adviser Detail");
            AdviserDetailRequest.SetIds(touchpointId, subcontractorId);


            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(AdviserDetailRequest, true);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to Create Adviser Detail"));
            var adviserdetail = await _AdviserDetailPostService.CreateAsync(AdviserDetailRequest);

            _loggerHelper.LogMethodExit(log);

            return adviserdetail == null
                ? _httpResponseMessageHelper.BadRequest()
                : _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(adviserdetail, "id", "AdviserDetailId"));

        }
    }
}