using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Function
{
    public class GetAdviserDetailByIdHttpTrigger
    {
        private readonly IGetAdviserDetailByIdHttpTriggerService _AdviserDetailGetService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger _logger;

        public GetAdviserDetailByIdHttpTrigger(
                IGetAdviserDetailByIdHttpTriggerService AdviserDetailGetService,
                ILoggerHelper loggerHelper,
                IHttpRequestHelper httpRequestHelper,
                ILogger<GetAdviserDetailByIdHttpTrigger> logger
            )
        {
            _AdviserDetailGetService = AdviserDetailGetService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.AdviserDetail), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Adviser Detail for the given customer")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AdviserDetails/{adviserDetailId}")] HttpRequest req, string adviserDetailId)
        {

            _loggerHelper.LogMethodEnter(_logger);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Unable to locate 'TouchpointId' in request header");                
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid,
                string.Format("Get AdviserDetail By Id C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));


            if (!Guid.TryParse(adviserDetailId, out var AdviserDetailGuid))
            {
                _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Unable to parse 'adviserDetailId' to a Guid: {0}", adviserDetailId));                
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(AdviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }


            _loggerHelper.LogInformationMessage(_logger, correlationGuid, string.Format("Attempting to get Adviser Detail", AdviserDetailGuid));
            var AdviserDetail = await _AdviserDetailGetService.GetAdviserDetailAsync(AdviserDetailGuid);

            _loggerHelper.LogMethodExit(_logger);            

            return AdviserDetail == null ?
                new NoContentResult() :
                new JsonResult(AdviserDetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}