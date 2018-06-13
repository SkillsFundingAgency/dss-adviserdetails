using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger
{
    public static class PostAdviserDetailHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.AdviserDetail))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdviserDetails")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Adviser Detail C# HTTP trigger function processed a request.");

            // Get request body
            var adviserDetail = await req.Content.ReadAsAsync<Models.AdviserDetail>();

            var adviserDetailService = new PostAdviserDetailHttpTriggerService();
            var adviserDetailId = adviserDetailService.Create(adviserDetail);

            return adviserDetailId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Adviser Detail record with Id of : " + adviserDetailId)
                };
        }
    }
}