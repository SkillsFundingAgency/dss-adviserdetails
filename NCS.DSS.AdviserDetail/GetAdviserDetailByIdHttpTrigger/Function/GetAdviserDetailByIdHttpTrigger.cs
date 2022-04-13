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
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Function
{
    public class GetAdviserDetailByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IGetAdviserDetailByIdHttpTriggerService _AdviserDetailGetService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public GetAdviserDetailByIdHttpTrigger(
                IResourceHelper resourceHelper,
                IGetAdviserDetailByIdHttpTriggerService AdviserDetailGetService,
                ILoggerHelper loggerHelper,
                IHttpRequestHelper httpRequestHelper,
                IHttpResponseMessageHelper httpResponseMessageHelper,
                IJsonHelper jsonHelper
            )
        {
            _resourceHelper = resourceHelper;
            _AdviserDetailGetService = AdviserDetailGetService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.AdviserDetail), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Adviser Detail for the given customer")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AdviserDetails/{adviserDetailId}")] HttpRequest req, ILogger log, string adviserDetailId)
        {

            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format($"Get AdviserDetail By Id C# HTTP trigger function  processed a request. By Touchpoint: {0} {touchpointId} and subcontractorId {subcontractorId}"));


            if (!Guid.TryParse(adviserDetailId, out var AdviserDetailGuid))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'adviserDetailId' to a Guid: {0}", adviserDetailId));
                return _httpResponseMessageHelper.BadRequest(AdviserDetailGuid);
            }


            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Adviser Detail", AdviserDetailGuid));
            var AdviserDetail = await _AdviserDetailGetService.GetAdviserDetailAsync(AdviserDetailGuid);

            _loggerHelper.LogMethodExit(log);

            return AdviserDetail == null ?
                _httpResponseMessageHelper.NoContent(AdviserDetailGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(AdviserDetail, "id", "AdviserDetailId"));

        }
    }
}