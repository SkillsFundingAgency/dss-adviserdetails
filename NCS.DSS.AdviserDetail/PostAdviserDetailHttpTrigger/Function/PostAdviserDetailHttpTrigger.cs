using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Annotations;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.Ioc;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Function
{
    public static class PostAdviserDetailHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Adviser Detail Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Adviser Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new adviser details resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdviserDetails")]HttpRequestMessage req, ILogger log,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostAdviserDetailHttpTriggerService adviserDetailsPostService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Adviser Detail C# HTTP trigger function processed a request. " + touchpointId);

            Models.AdviserDetail adviserDetailRequest;

            try
            {
                adviserDetailRequest = await httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (adviserDetailRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            adviserDetailRequest.LastModifiedTouchpointId = touchpointId;

            var errors = validate.ValidateResource(adviserDetailRequest, true);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var adviserDetail = await adviserDetailsPostService.CreateAsync(adviserDetailRequest);

            return adviserDetail == null
                ? HttpResponseMessageHelper.BadRequest()
                : HttpResponseMessageHelper.Created(JsonHelper.SerializeObject(adviserDetail));
        }
    }
}