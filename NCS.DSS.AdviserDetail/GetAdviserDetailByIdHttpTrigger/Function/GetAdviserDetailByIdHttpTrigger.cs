using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Function
{
    public class GetAdviserDetailByIdHttpTrigger
    {
        private readonly IGetAdviserDetailByIdHttpTriggerService _AdviserDetailGetService;
        
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger<GetAdviserDetailByIdHttpTrigger> _logger;

        public GetAdviserDetailByIdHttpTrigger(
                IGetAdviserDetailByIdHttpTriggerService AdviserDetailGetService,
                IHttpRequestHelper httpRequestHelper,
                ILogger<GetAdviserDetailByIdHttpTrigger> logger
            )
        {
            _AdviserDetailGetService = AdviserDetailGetService;
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
            var functionName = nameof(GetAdviserDetailByIdHttpTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogWarning("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogError("{CorrelationGuid} Unable to locate 'TouchpointId' in request header",correlationId);
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("{CorrelationGuid} Get AdviserDetail By Id C# HTTP trigger function  processed a request. By Touchpoint: {touchpointId}", correlationId,touchpointId);


            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                _logger.LogError("{CorrelationGuid} Unable to parse 'adviserDetailId' to a Guid: {adviserDetailId}", correlationId,adviserDetailId);
                return new BadRequestObjectResult(new StringContent(JsonConvert.SerializeObject(adviserDetailGuid), Encoding.UTF8, ContentApplicationType.ApplicationJSON));
            }


            _logger.LogInformation("{CorrelationGuid} Attempting to get Adviser Detail {AdviserDetailGuid}", correlationId,adviserDetailId);
            var AdviserDetail = await _AdviserDetailGetService.GetAdviserDetailAsync(adviserDetailGuid);

            if (AdviserDetail == null)
            {
                _logger.LogError("{CorrelationGuid} Adviser Detail does not exist {AdviserDetailId}", correlationGuid, adviserDetailGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);

            return new JsonResult(AdviserDetail, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}